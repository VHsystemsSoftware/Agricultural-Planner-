namespace VHS.Common
{
	public static class GlobalConstants
	{
		public enum DestinationEnum
		{
			NONE=0,
			DESTINATION_PATERNOSTER = -1,
			DESTINATION_TRANSPLANTER = -2,
			DESTINATION_WASHER = -3,
			DESTINATION_HARVESTER = -4,
			DESTINATION_PROPAGATION = -5,
			DESTINATION_GERMINATIONRACK1 = 1,
			DESTINATION_GERMINATIONRACK2 = 2,
			DESTINATION_GERMINATIONRACK3 = 3
		}

		public static Guid BATCHPLANSTATUS_NEW = new Guid("a3e8e89c-12f1-4cd4-a0f3-b4deaad1d7f0");
		public static Guid BATCHPLANSTATUS_PLANNED = new Guid("d1e04e33-5e1e-4c68-91b5-6c0e8e3242d5");
		public static Guid BATCHPLANSTATUS_ACTIVE = new Guid("28e29b88-03af-45c8-89c0-48d20b5f476f");
		public static Guid BATCHPLANSTATUS_FINISHED = new Guid("baf5c5cb-b5d0-4e09-9e6c-2b0cdd537b2d");

		public static Guid BATCHSTATUS_PLANNED = new Guid("afca571c-fa99-4040-becb-8412889b097f");
		public static Guid BATCHSTATUS_COMPLETED = new Guid("3d6036ea-63c7-441d-aba8-6a8b677bc0a4");
		public static Guid BATCHSTATUS_CANCELLED = new Guid("01909ed6-e9db-435d-a3c3-cccc0c614a0d");

		public static Guid JOBSTATUS_NOTSTARTED = new Guid("a592ab99-3141-44c9-83ed-6fabb9534100");
		public static Guid JOBSTATUS_INPROGRESS = new Guid("cb99482d-6751-4990-a844-17e23b070963");
		public static Guid JOBSTATUS_COMPLETED = new Guid("227d3c9e-605c-401e-99f9-1ac80303aebe");
		public static Guid JOBSTATUS_PAUSED = new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d");
		public static Guid JOBSTATUS_CANCELLED = new Guid("d946509e-e486-4349-aad9-f452e30b4ad0");


		public static Guid RACKTYPE_GROWING = new Guid("4a478cc6-a4dd-4210-9f09-e02cbf0fbe81");
		public static Guid RACKTYPE_GERMINATION = new Guid("7b901e37-803e-45d8-833d-4a6dcf77f172");
		public static Guid RACKTYPE_PROPAGATION = new Guid("b77de880-5105-49eb-8a1a-43f1bd7bf15e");

		public static Guid PRODUCTCATEGORY_LETTUCE = new Guid("90e1ee66-1949-4ffd-88a4-3809c32739de");
		public static Guid PRODUCTCATEGORY_MICROGREENS = new Guid("3a0c8cc4-18a8-41e2-afc9-4b51fc288f1c");
		public static Guid PRODUCTCATEGORY_PETITEGREENS = new Guid("173fc70b-3475-4303-b9bd-c49870af13bf");

		public static Guid TRAYSTATUS_INUSE = new Guid("b146c4f2-05f5-4a36-9b5d-73b8f2b5d18f");
		public static Guid TRAYSTATUS_BROKEN = new Guid("e2d2c3e9-9b47-499e-b8e6-3b74636bf9d5");
		public static Guid TRAYSTATUS_REMOVED = new Guid("9f3c324d-d59c-4707-a5e2-6a6a34fc7c5d");

		public static Guid JOBLOCATION_SEEDER = new Guid("29e3acf3-c85f-4fc7-9aa5-d5fe94fb4606");
		public static Guid JOBLOCATION_HARVESTER = new Guid("b81fbc7b-95c1-4038-83ec-68e134215d71");
		public static Guid JOBLOCATION_TRANSPLANTER = new Guid("311f2a19-0bf7-4882-8863-250bece8b988");

		public static Guid BATCHPLANTYPE_RACK = new Guid("4eb3259f-33da-4f78-8263-aaeca9bc7fc4");
		public static Guid BATCHPLANTYPE_WASHER = new Guid("9c3f2e55-6ca1-46bd-aec9-8db66dd8e8a1");
		public static Guid BATCHPLANTYPE_TRANSPLANTER = new Guid("ce7a0030-6b29-46a9-8878-a11b162dc7dc");
		public static Guid BATCHPLANTYPE_RECIPE = new Guid("c8f9d9dd-9cf7-490d-acb6-0449580fa3b0");

		public static Guid TRAYSTATE_EMPTYREASON_UNKNOW = new Guid("b9a07222-2a84-498d-b379-41d114bb3e5c");
		public static Guid TRAYSTATE_EMPTYREASON_LAYERFILL = new Guid("0959555d-2b7c-4c6c-ac81-4094105b3ec4");
		public static Guid TRAYSTATE_EMPTYREASON_TOTRANSPLANT = new Guid("d3f8c5b1-2e4a-4c0b-9f6d-7c8e1f2b3a4b");
		public static Guid TRAYSTATE_EMPTYREASON_HARVESTED = new Guid("f8b9d2e3-1c4a-4f0b-b5d6-8e1f2b3a4c5d");
		public static Guid TRAYSTATE_EMPTYREASON_FROMTRANSPLANT = new Guid("c2d3e4f5-6a7b-8c9d-0e1f-2a3b4c5d6e7f");
		public static Guid TRAYSTATE_EMPTYREASON_TOWASHING = new Guid("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d");

		public static Guid JOBTYPE_SEEDING_GERMINATION = new Guid("d1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a");
		public static Guid JOBTYPE_SEEDING_PROPAGATION = new Guid("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d");
		public static Guid JOBTYPE_HARVESTING = new Guid("b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e");
		public static Guid JOBTYPE_EMPTY_TOTRANSPLANT = new Guid("c2d3e4f5-6a7b-8c9d-0e1f-2a3b4c5d6e7f");
		//public static Guid JOBTYPE_TRANSPLANTING = new Guid("f8b9d2e3-1c4a-4f0b-b5d6-8e1f2b3a4c5d");
		public static Guid JOBTYPE_EMPTY_TOWASHER = new Guid("e1f2a3b4-c5d6-7e8f-9a0b-1c2d3e4f5a6b");
		public static Guid JOBTYPE_EMPTY_TORACK = new Guid("d3f8c5b1-2e4a-4c0b-9f6d-7c8e1f2b3a4b");

		// System Messages
		public static Guid SYSTEM_MESSAGE_SEVERITY_INFO = new Guid("1f2a3b4c-5d6e-4f78-9a0b-1c2d3e4f5a6b");
		public static Guid SYSTEM_MESSAGE_SEVERITY_WARNING = new Guid("a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6");
		public static Guid SYSTEM_MESSAGE_SEVERITY_ERROR = new Guid("f9e8d7c6-b5a4-43f2-9180-7e6d5c4b3a2f");

		public static Guid SYSTEM_MESSAGE_CATEGORY_OPC = new Guid("1b1e8787-11f1-438e-a279-994c5e3d7e4b");
		public static Guid SYSTEM_MESSAGE_CATEGORY_JOBS = new Guid("2a8f9b3c-5d1e-4f8a-b8e2-c5d9a7f0b3e1");
		public static Guid SYSTEM_MESSAGE_CATEGORY_PLANNING = new Guid("3c4d5e6f-7a8b-4c9d-8e1f-2a3b4c5d6e7f");
		public static Guid SYSTEM_MESSAGE_CATEGORY_DATA = new Guid("4d5e6f7a-8b9c-4d0e-9f1a-2b3c4d5e6f7a");
		public static Guid SYSTEM_MESSAGE_CATEGORY_FARM = new Guid("5e6f7a8b-9c0d-4e1f-a2b3-c4d5e6f7a8b9");
		public static Guid SYSTEM_MESSAGE_CATEGORY_GROWTH = new Guid("6f7a8b9c-0d1e-4f2a-b3c4-d5e6f7a8b9c0");
		public static Guid SYSTEM_MESSAGE_CATEGORY_PRODUCT = new Guid("7a8b9c0d-1e2f-4a3b-84d5-e6f7a8b9c0d1");
		public static Guid SYSTEM_MESSAGE_CATEGORY_SECURITY = new Guid("8b9c0d1e-2f3a-4b4c-95e6-f7a8b9c0d1e2");

	}
}
