namespace Apps.Apollo.Models.Dtos;

public record PaginatedResult<T>(List<T> Items, long? TotalEntries);
