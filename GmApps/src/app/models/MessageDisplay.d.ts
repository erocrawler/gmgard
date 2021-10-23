interface MessageDisplay {
  messageId: number;
  isRead: boolean;
  title: string;
  sendDate: Date;
  quickLink: string;
  quickText: string;
  sender: string;
  senderNickName: string;
  recipient: string;
  recipientNickName: string;
}

interface MessageDetails extends MessageDisplay {
  content: string;
  senderAvatar: string;
  senderLink: string;
  recipientAvatar: string;
  recipientLink: string;
}
