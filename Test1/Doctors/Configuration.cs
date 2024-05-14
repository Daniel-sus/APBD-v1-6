namespace Test1.Doctors;

public static class Configuration
{
    public static IEndpointRouteBuilder RegisterDoctorsUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/doctors", (IDoctorsRepository repository) => TypedResults.Ok(repository.GetDoctors()));
        endpoints.MapGet("/doctors/{id:int}", (int id, IDoctorsRepository repository) => TypedResults.Ok(repository.GetDoctorWithPrescriptions(id)));
        endpoints.MapDelete("/doctors/{id:int}", (int id, IDoctorsRepository repository) =>
        {
            repository.DeleteDoctorAndPrescriptions(id);
            return TypedResults.NoContent();
        });

        return endpoints;
    }
}