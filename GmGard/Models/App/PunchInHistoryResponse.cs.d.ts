	interface punchIn {
		timeStamp: Date;
		isMakeUp: boolean;
	}
	interface punchInHistoryResponse {
		punchIns: punchIn[];
		legacySignDays?: number;
		minSignDate: Date;
	}
