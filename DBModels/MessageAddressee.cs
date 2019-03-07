using System.ComponentModel.DataAnnotations;

namespace DBModels
{
    public class MessageAddressee
    {
        [Key]
        public string PushMessageID { get; set; }
        public string application_no { get; set; }
        public string contract_number { get; set; }
        public string Addressee { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Attachments { get; set; }
        public string SendedMessageTime { get; set; }
        public string ReceivedMessageTime { get; set; }
        public string CreatedDate { get; set; }
    }
}
