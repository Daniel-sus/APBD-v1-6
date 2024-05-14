namespace Test1.Doctors
{
    public class Prescription
    {
        public int IdPrescription { get; set; }
        public DateTime Date { get; set; }
        public List<Medicament> Medicaments { get; set; }
    }
}