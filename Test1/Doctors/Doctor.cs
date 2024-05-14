namespace Test1.Doctors;
public class Doctor
{
    public int IdDoctor { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<Prescription> Prescriptions { get; set; }

}