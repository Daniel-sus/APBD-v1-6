namespace Test1.Doctors;

public interface IDoctorsRepository
{
    IEnumerable<Doctor> GetDoctors();

    Doctor GetDoctorWithPrescriptions(int id);

    void DeleteDoctorAndPrescriptions(int id);
}