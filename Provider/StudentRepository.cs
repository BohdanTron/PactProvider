namespace Provider
{
    public interface IStudentRepository
    {
        Student? GetById(int id);
        Student Add(Student student);
    }

    public class StudentRepository : IStudentRepository
    {
        private readonly List<Student> _students = [];

        public Student? GetById(int id)
        {
            return _students.FirstOrDefault(x => x.Id == id);
        }

        public Student Add(Student student)
        {
            _students.Add(student);
            return student;
        }
    }
}
