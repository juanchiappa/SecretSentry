namespace SecretSentry.DataAccess;

/// <summary>
/// Storage-agnostic persistence contract. Business logic depends
/// only on this interface, never on a concrete backend.
/// </summary>
public interface IPersistenceProvider
{
    List<T> GetAll<T>() where T : class;
    T? GetById<T>(int id) where T : class;
    void Save<T>(T entity) where T : class;
    void Delete<T>(int id) where T : class;
}
