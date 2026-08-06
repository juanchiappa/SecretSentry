using SecretSentry.Entities;

namespace SecretSentry.BusinessLogic.Scanning;

public interface IRepositoryScanner
{
    List<Finding> Scan(string repositoryPath);
}