using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.Threading.Tasks;
using VHS.Data.Core.Models;
using VHS.Services.Batches.DTO;
using VHS.Services.Farming.DTO;

namespace VHS.Services.Farming.Algorithm
{
	public interface IGrowPlanAlgoritmeService
	{
		Task<List<GrowDemandResultPerDay>> GrowDemands(Guid farmId, GrowPlanDTO growPlan, int rackSize, int layers, bool cleanGrowRack = false);
	}

	public class GrowPlanAlgoritmeService : IGrowPlanAlgoritmeService
	{
		private readonly IFarmService FarmService;
		private readonly IBatchService BatchService;
		private readonly IGrowPlanService GrowPlanService;
		private readonly IRecipeService RecipeService;

		public GrowPlanAlgoritmeService(IFarmService farmService, IBatchService batchService, IGrowPlanService growPlanService, IRecipeService recipeService)
		{
			FarmService = farmService;
			BatchService = batchService;
			GrowPlanService = growPlanService;
			RecipeService = recipeService;
		}

		public async Task<List<GrowDemandResultPerDay>> GrowDemands(Guid farmId, GrowPlanDTO growPlan, int rackSize, int layers, bool cleanRack = false)
		{
			List<GrowDemandResultPerDay> outputList = new List<GrowDemandResultPerDay>();

			var farm = await FarmService.GetFarmByIdAsync(farmId);

			var recipe = await RecipeService.GetRecipeByIdAsync(growPlan.Recipe.Id);
			growPlan.Recipe = recipe;

			var germinationRacks = farm.GerminationRacks.Where(x => x.Enabled && x.Floor.Enabled).OrderBy(x => x.TrayCountPerLayer).ThenBy(x => x.Number).ToList();
			var propagationRacks = farm.PropagationRacks.Where(x => x.TrayCountPerLayer == rackSize && x.Enabled && x.Floor.Enabled).OrderBy(x => x.TrayCountPerLayer).ThenBy(x => x.Number).ToList();
			var growRacks = farm.GrowRacks.Where(x => x.TrayCountPerLayer == rackSize && x.Enabled && x.Floor.Enabled).OrderBy(x => x.TrayCountPerLayer).ThenBy(x => x.Number).ToList();
			var maxDay = growPlan.StartDate.Value.AddDays(growPlan.DaysForPlan + growPlan.Recipe.GerminationDays + growPlan.Recipe.GrowDays + growPlan.Recipe.PropagationDays);
			var startDate = growPlan.StartDate.Value;
			var activeBatches = await BatchService.GetActiveBatches(farmId, startDate);

			List<GrowPlanDTO> growPlans = new List<GrowPlanDTO>() { growPlan };
			
			var IsPropagation = recipe.Product.ProductCategoryId == GlobalConstants.PRODUCTCATEGORY_LETTUCE;
			var usePreGrowRacks = IsPropagation ? propagationRacks : germinationRacks;

			int i = 0;
			while (startDate.AddDays(i) < maxDay)
			{
				GrowDemandResultPerDay output = new GrowDemandResultPerDay()
				{
					Date = startDate.AddDays(i),
					DayCount = i + 1,
					Batches = new()
				};

				DateOnly futureDate = startDate.AddDays(i);

				i++;
				int harvested = 0;
				int germDone = 0;
				int germWasher = 0;



				var allRunningPlans = await GrowPlanService.GetAllGrowPlansAsync(null, null, null, futureDate, null, null);
				growPlans.AddRange(allRunningPlans); 
				growPlans = growPlans.Where(x => x.StartDate <= futureDate).OrderBy(x => x.StartDate).ThenBy(x => x.AddedDateTime).ToList();

				foreach (var gp in growPlans)
				{
					var growPlansRunning = gp.StartDate.Value.AddDays(gp.DaysForPlan) > futureDate;

					var germFinishedLayers = usePreGrowRacks.SelectMany(x => x.Layers).Where(x => x.Enabled 
						&& (IsPropagation ? x.IsFinishedPropagation(futureDate) : x.IsFinishedGermination(futureDate)) && x.BatchId.HasValue && x.Batch.GrowPlan.Id == gp.Id)
						.OrderBy(x => x.Rack.Number)
						.ThenByDescending(x => x.Number).ToList();
					var germFinishedBatches = germFinishedLayers.Select(x => x.Batch).Distinct().ToList();

					List<BatchDTO> useBatches = new();

					if (germFinishedLayers.Any() && !growPlansRunning)
					{
						foreach (var useBatch in germFinishedBatches)
						{
							var emptyBatch = new BatchDTO()
							{
								Id = Guid.NewGuid(),
								SeedDate = startDate.AddDays(i - 1),
								HarvestDate = null,
								Name = $"Empty {useBatch.Name}",
								TrayCount = useBatch.TrayCount,
								FarmId = farmId,
								GrowPlan = useBatch.GrowPlan,
								Recipe = null
							};
							useBatches.Add(emptyBatch);
							output.Batches.Add(emptyBatch);
							output.Messages.Add($"Batch (empty) [{emptyBatch.Name}] with [{emptyBatch.TrayCount}] trays for growplan [{gp.Name}] on day [{i}]");
						}

					}
					else if (growPlansRunning)
					{
						var newBatch = new BatchDTO()
						{
							Id = Guid.NewGuid(),
							SeedDate = startDate.AddDays(i - 1),
							HarvestDate = gp.Recipe==null ? null : startDate.AddDays(i - 1).AddDays(gp.Recipe.GrowDays + gp.Recipe.GerminationDays + gp.Recipe.PropagationDays),
							Name = $"Day-{i}-{(gp.Recipe == null ? "empty" : gp.Name)}",
							TrayCount = gp.TraysPerDay,
							FarmId = farmId,
							GrowPlan = gp,
							Recipe = gp.Recipe
						};
						useBatches.Add(newBatch);
						output.Batches.Add(newBatch);

						output.Messages.Add($"Batch [{newBatch.Name}] with [{newBatch.TrayCount}] trays for plan [{gp.Name}] on day [{i}]");
					}


					foreach (var useBatch in useBatches)
					{
						List<LayerDTO> useGermLayers;

						var germFinishedSameGPLayers = usePreGrowRacks.SelectMany(x => x.Layers).Where(x => x.Enabled 
						&& (IsPropagation ? x.IsFinishedPropagation(futureDate) : x.IsFinishedGermination(futureDate)) && x.Batch.GrowPlan.Id == gp.Id).OrderBy(x => x.Rack.Number).ThenByDescending(x => x.Number).ToList();
						var germEmptyLayers = usePreGrowRacks.SelectMany(x => x.Layers).Where(x => x.Enabled && x.IsEmpty).OrderBy(x => x.Rack.Number).ThenByDescending(x => x.Number).ToList();

						//if no demand is running empty the germination layers with empty ones to push to growing
						useGermLayers = growPlansRunning ? (germFinishedSameGPLayers.Any() ? germFinishedSameGPLayers : germEmptyLayers) : germFinishedLayers;

						if (useGermLayers.Any() && useGermLayers.Sum(x => x.TrayCountPerLayer) >= gp.TraysPerDay)
						{
							var germData = MoveToPreGrow(usePreGrowRacks.SelectMany(x => x.Layers).ToList(), useGermLayers, useBatch, (
								gp.Batches.Any()
								? gp.Batches.First().BatchRows.Where(x => x.LayerRackTypeId == GlobalConstants.RACKTYPE_GERMINATION).ToList()
								: new List<BatchRowDTO>()), layers);

							output.Messages.Add($"Passing seeder [{useBatch.BatchRows.Sum(x => x.TrayCount)}] trays on [{useBatch.BatchRows.Count()}] layers to pregrow for batch [{useBatch.Name}] on day [{i}]");

							germDone = germData.germDone;
							germWasher = germData.washer;

							if (germData.germinatedBatch != null)
							{
								var growFinishedSameGPLayers = growRacks.SelectMany(x => x.Layers)
										.Where(x => x.Enabled
												&& x.IsFinishedGrowing(futureDate)
												&& x.Batch.GrowPlan.Id == gp.Id)
										.OrderBy(x => x.Rack.Number).ThenByDescending(x => x.Number).ToList();
								var growEmptyLayers = growRacks.SelectMany(x => x.Layers).Where(x => x.Enabled && x.IsEmpty).OrderBy(x => x.Rack.Number).ThenByDescending(x => x.Number).ToList();
								var useGrowLayers = growFinishedSameGPLayers.Any() ? growFinishedSameGPLayers : growEmptyLayers;

								output.Messages.Add($"Moving [{germDone - germWasher}] trays to growing for batch [{germData.germinatedBatch.Name}] on day [{i}]");
								if (germWasher > 0) output.Messages.Add($"Washing [{germWasher}] trays for [{germData.germinatedBatch.Name}] on day [{i}]");

								if (useGrowLayers.Any())
								{
									var growData = MoveToGrowing(growRacks.SelectMany(x => x.Layers).Where(x => x.Enabled).ToList(), germData.germinatedBatch, useGrowLayers);
									harvested += growData.harvested;
									if (harvested > 0) output.Messages.Add($"Harvested [{harvested}] trays from growing for batch [{germData.germinatedBatch.Name}] on day [{i}]");
								}
								else
								{
									output.Errors.Add($"No space in growing for batch [{germData.germinatedBatch.Name}] on day [{i}]");
									maxDay = startDate.AddDays(i - 1);
									break;
								}
							}
						}
						else
						{
							output.Errors.Add($"No space in pregrow for batch [{useBatch.Name}] on day [{i}]");
							maxDay = startDate.AddDays(i - 1);
							break;
						}
					}
				}

				//check if any growing finished without new batch is pushing from germination                    
				var finalGrowFinishedLayers = growRacks.SelectMany(x => x.Layers).Where(x => x.Enabled && x.IsFinishedGrowing(futureDate)).OrderBy(x => x.Rack.Number).ThenByDescending(x => x.Number).ToList();
				if (finalGrowFinishedLayers.Any())
				{
					output.Messages.Add("There are finished growing layers but no new batch is planned to push from germination");
				}

				output.Harvested = harvested;
				output.Washed = germWasher;

				var germRacksClone = DeepClone(usePreGrowRacks.Where(x => x.Layers.Any(x => x.BatchId.HasValue))).ToList();
				germRacksClone.ForEach(rack => rack.Layers = rack.Layers.Where(x => rack.TypeId != GlobalConstants.RACKTYPE_GERMINATION || (rack.TypeId == GlobalConstants.RACKTYPE_GERMINATION && x.BatchId.HasValue)).ToList());

				var growRacksClone = DeepClone(growRacks.Where(x => x.Layers.Any(x => x.BatchId.HasValue))).ToList();
				output.Racks = germRacksClone.Concat(growRacksClone).ToList();

				outputList.Add(output);
			}

			return outputList;
		}		

		public static T DeepClone<T>(T obj)
		{
			var json = JsonSerializer.Serialize(obj);
			return JsonSerializer.Deserialize<T>(json);
		}

		private (BatchDTO germinatedBatch, int germDone, int washer) MoveToPreGrow(List<LayerDTO> allLayers, List<LayerDTO> germLayers, BatchDTO newBatch, List<BatchRowDTO> existingBatchRows, int growLayersNeeded)
		{
			var germLayersFilled = 0;
			var germDone = 0;
			var washerDone = 0;
			BatchDTO germinatedBatch = null;
			int germinationLayersNeeded = (int)Math.Ceiling((decimal)newBatch.TrayCount / germLayers.First().Rack.TrayCountPerLayer);

			germLayers = germLayers.OrderBy(x => x.Rack.Number).ThenByDescending(x => x.Number).ToList();
			foreach (var item in existingBatchRows)
			{
				item.Layer = allLayers.Find(x => x.Id == item.LayerId.Value);
			}

			for (int i = 0; i < germLayers.Count; i++)
			{
				if (germLayersFilled < germinationLayersNeeded)
				{
					var moveLayer = germLayers[i];
					if (existingBatchRows.Any()) germinationLayersNeeded = existingBatchRows.Count;

					var isInBatch = newBatch?.BatchRows != null ? newBatch.BatchRows.Any(x => x.LayerId == moveLayer.Id) : false;

					germDone += moveLayer.Rack.TrayCountPerLayer;
					var traysNeeded = germinationLayersNeeded * moveLayer.Rack.TrayCountPerLayer;
					var transportLayer = allLayers.Single(x => x.Id == moveLayer.Rack.TransportLayer.Id);

					var moveToLayer = moveLayer.IsTransportLayer
							? germLayers[i + 1]
							: moveLayer;

					if (moveToLayer.Batch != null && !isInBatch)
					{
						//finished growing so will be pushed out
						germinatedBatch = moveToLayer.Batch;
						washerDone = (germDone > traysNeeded) ? germDone - traysNeeded : 0;
						moveToLayer.BatchId = null;
					}

					newBatch.BatchRows.Add(new BatchRowDTO()
					{
						Id = Guid.NewGuid(),
						BatchId = newBatch.Id,
						FloorId = moveToLayer.Rack.FloorId,
						RackId = moveToLayer.RackId,
						LayerId = moveToLayer.Id,
						Layer = moveToLayer,
						Rack = moveToLayer.Rack,
						Number = newBatch.BatchRows.Count() + 1,
						TrayCount = moveToLayer.Rack.TrayCountPerLayer,
						EmptyCount = (germDone > traysNeeded) ? germDone - traysNeeded : 0,
						AddedDateTime = DateTime.UtcNow,
					});
					germLayersFilled++;

					if (!moveLayer.IsTransportLayer)
					{
						moveLayer.BatchId = transportLayer.Batch?.Id;
						moveLayer.Batch = transportLayer.Batch;
						moveLayer.BatchRowNumber = transportLayer.BatchRowNumber;
					}
					transportLayer.BatchId = newBatch?.Id;
					transportLayer.Batch = newBatch;
					transportLayer.BatchRowNumber = newBatch.BatchRows.Count();

					if (moveLayer.IsTransportLayer && newBatch.Recipe == null)
					{
						//batch is empty and movelayer is transport so set seed date to today
						newBatch.BatchRows.Add(new BatchRowDTO()
						{
							Id = Guid.NewGuid(),
							BatchId = newBatch.Id,
							FloorId = moveToLayer.Rack.FloorId,
							RackId = moveToLayer.RackId,
							LayerId = moveToLayer.Id,
							Layer = moveToLayer,
							Rack = moveToLayer.Rack,
							Number = newBatch.BatchRows.Count() + 1,
							TrayCount = moveToLayer.Rack.TrayCountPerLayer,
							EmptyCount = (germDone > traysNeeded) ? germDone - traysNeeded : 0,
							AddedDateTime = DateTime.UtcNow,
						});

						moveToLayer.BatchId = transportLayer.Batch?.Id;
						moveToLayer.Batch = transportLayer.Batch;
						moveToLayer.BatchRowNumber = transportLayer.BatchRowNumber;

						transportLayer.BatchId = newBatch?.Id;
						transportLayer.Batch = newBatch;
						transportLayer.BatchRowNumber = newBatch.BatchRows.Count();

						washerDone = (germDone > traysNeeded) ? germDone - traysNeeded : 0;
					}
				}
			}

			return (germinatedBatch, germDone, washerDone);
		}

		private (int harvested, int growTraysFilled) MoveToGrowing(List<LayerDTO> allLayers, BatchDTO germinatedBatch, List<LayerDTO> growLayers)
		{
			int harvested = 0;
			int growTraysFilled = 0;
			var traysNeeded = germinatedBatch != null ? germinatedBatch.TrayCount : growLayers.Sum(x => x.Rack.TrayCountPerLayer);

			for (int i = 0; i < growLayers.Count; i++)
			{
				var moveLayer = growLayers[i];

				if (growTraysFilled < traysNeeded)
				{
					var transportLayer = allLayers.Single(x => x.Id == moveLayer.Rack.TransportLayer.Id);

					if (moveLayer.Batch != null)
					{
						//finished growing so will be pushed out
						harvested += moveLayer.Rack.TrayCountPerLayer;
					}

					var moveToLayer = moveLayer.IsTransportLayer ? (i < growLayers.Count ? growLayers[i + 1] : moveLayer) : moveLayer;

					germinatedBatch.BatchRows.Add(new BatchRowDTO()
					{
						Id = Guid.NewGuid(),
						BatchId = germinatedBatch.Id,
						FloorId = moveLayer.Rack.FloorId,
						Rack = moveLayer.Rack,
						RackId = moveLayer.RackId,
						LayerId = moveToLayer.Id,
						Layer = moveToLayer,
						Number = moveLayer.Number,
						TrayCount = moveLayer.Rack.TrayCountPerLayer,
						EmptyCount = 0
					});

					moveLayer.BatchId = transportLayer.Batch?.Id;
					moveLayer.Batch = transportLayer.Batch;
					transportLayer.BatchId = germinatedBatch?.Id;
					transportLayer.Batch = germinatedBatch;

					growTraysFilled += moveLayer.Rack.TrayCountPerLayer;
				}
			}

			return (harvested, growTraysFilled);
		}
	}
}