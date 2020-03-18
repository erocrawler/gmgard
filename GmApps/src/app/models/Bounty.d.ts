export interface BountyPreview {
    id: number;
    createDate: Date;
    author: string;
    authorAvatar: string;
    title: string;
    content: string;
    image: string;
    prize: number;
    isAccepted: boolean;
}

export type BountyShowType = 'All' | 'Answered' | 'Pending';
