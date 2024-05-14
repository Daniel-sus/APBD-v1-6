using System.Data.SqlClient;

namespace Test1.Doctors;

public class DoctorsRepository : IDoctorsRepository
{
    private IConfiguration _configuration;

    public DoctorsRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IEnumerable<Doctor> GetDoctors()
    {
        using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        con.Open();

        using var cmd = new SqlCommand();
        cmd.Connection = con;

        cmd.CommandText = "SELECT FirstName FROM Doctor";
        var dr = cmd.ExecuteReader();
        var doctors = new List<Doctor>();
        while (dr.Read())
        {
            var doctor = new Doctor
            {
                FirstName = dr["FirstName"].ToString(),
            };
            doctors.Add(doctor);
        }
        return doctors;
    }

    public Doctor GetDoctorWithPrescriptions(int doctorId)
    {
        using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        con.Open();

        using var cmd = new SqlCommand();
        cmd.Connection = con;

        cmd.CommandText = @"SELECT D.IdDoctor, D.FirstName, D.LastName, P.IdPrescription, P.Date, PM.IdMedicament, M.Name AS MedicamentName, PM.Dose, PM.Details
                                FROM Doctor D
                                LEFT JOIN Prescription P ON D.IdDoctor = P.IdDoctor
                                LEFT JOIN Prescription_Medicament PM ON P.IdPrescription = PM.IdPrescription
                                LEFT JOIN Medicament M ON PM.IdMedicament = M.IdMedicament
                                WHERE D.IdDoctor = @DoctorId
                                ORDER BY P.Date DESC";
        cmd.Parameters.AddWithValue("@DoctorId", doctorId);

        var dr = cmd.ExecuteReader();
        var doctor = new Doctor();
        var prescriptions = new List<Prescription>();

        while (dr.Read())
        {
            if (doctor.IdDoctor == 0)
            {
                doctor.IdDoctor = Convert.ToInt32(dr["IdDoctor"]);
                doctor.FirstName = dr["FirstName"].ToString();
                doctor.LastName = dr["LastName"].ToString();
            }

            if (dr["IdPrescription"] != DBNull.Value)
            {
                var prescriptionId = Convert.ToInt32(dr["IdPrescription"]);
                var prescription = prescriptions.Find(p => p.IdPrescription == prescriptionId);

                if (prescription == null)
                {
                    prescription = new Prescription
                    {
                        IdPrescription = prescriptionId,
                        Date = Convert.ToDateTime(dr["Date"]),
                        Medicaments = new List<Medicament>()
                    };
                    prescriptions.Add(prescription);
                }

                if (dr["IdMedicament"] != DBNull.Value)
                {
                    var medicament = new Medicament
                    {
                        IdMedicament = Convert.ToInt32(dr["IdMedicament"]),
                        Name = dr["MedicamentName"].ToString(),
                        // Description = dr["Description"].ToString(),
                        // Type = dr["Type"].ToString()
                    };
                    prescription.Medicaments.Add(medicament);
                }
            }
        }

        if (doctor.IdDoctor == 0)
        {
            throw new Exception("Doctor not found");
        }

        doctor.Prescriptions = prescriptions;
        return doctor;
    }

    public int DeleteDoctorAndPrescriptions(int doctorId)
    {
        using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        con.Open();

        using var transaction = con.BeginTransaction();
        try
        {
            int affectedCount = 0;

            // Delete prescriptions associated with the doctor
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = "DELETE FROM Prescription_Medicament WHERE IdPrescription IN (SELECT IdPrescription FROM Prescription WHERE IdDoctor = @DoctorId)";
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                affectedCount += cmd.ExecuteNonQuery();
            }

            // Delete prescriptions of the doctor
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = "DELETE FROM Prescription WHERE IdDoctor = @DoctorId";
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                affectedCount += cmd.ExecuteNonQuery();
            }

            // Delete the doctor
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = "DELETE FROM Doctor WHERE IdDoctor = @DoctorId";
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                affectedCount += cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            return affectedCount;
        }
        catch (SqlException exc)
        {
            //...
            transaction.Rollback();
            return exc;
        }
        catch (Exception exc)
        {
            transaction.Rollback();
            throw Exception;
        }

    }
}


