namespace Provider
{
    public class Student
    {
        public required int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Address { get; set; }
        public required string Gender { get; set; }
        public Standard Standard { get; set; }
    }
    public enum Standard
    {
        Standard1 = 1,
        Standard2,
        Standard3,
        Standard4,
        Standard5,
        Standard6,
        Standard7
    }
}
