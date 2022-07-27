
namespace LightenceClient.Models
{
    public class User
    {
        public string FirstLastName { get; set; }
        public string Email { get; set; }
        public string ID { get; set; }
        public byte[] Photo { get; set; }
        public bool Premium { get; set; }
    }
}
