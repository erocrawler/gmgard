declare module server {
	interface BlogDetails {
		id: number;
		title: string;
		brief: string;
		content: string;
		imageUrls: string[];
		thumbUrl: string;
		createDate: Date;
		categoryId: number;
		parentCategoryId?: number;
		author: {
			userId: number;
			userName: string;
			nickName: string;
			points: number;
			experience: number;
			level: number;
			avatar: string;
			comment: string;
			roles: string[];
		};
		authorDesc: string;
		isApproved?: boolean;
		visitCount: number;
		links: {
			name: string;
			url: string;
			pass: string;
		}[];
		rating: {
			blogId: number;
			total: number;
			count: number;
			countByRating: any[];
			average: number;
		};
		tags: { [index: number]: string };
		topComments: {
			type: any;
			itemId: number;
			commentId: number;
			content: string;
			author: string;
			createDate: Date;
			rating?: number;
			upvoteCount: number;
			downvoteCount: number;
			isUpvoted?: boolean;
			isDownvoted?: boolean;
			replies: any[];
		}[];
		lockTags: boolean;
		noRate: boolean;
		noComment: boolean;
		isFavorite: boolean;
		userRating?: number;
	}
}
