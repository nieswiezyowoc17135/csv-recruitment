using csv_project.Data.DTOs;
using MediatR;

namespace csv_project.Features.Query;

public record CsvReadQuery(string? Order, DateTime? FromWhen, DateTime? ToWhen, List<string>? ClientCodes) : IRequest<CsvReadQueryResult>;

public class CsvReader
{
    public class CsvReaderQueryHandler : IRequestHandler<CsvReadQuery,CsvReadQueryResult>
    {
        public Task<CsvReadQueryResult> Handle(CsvReadQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

public record CsvReadQueryResult(IEnumerable<OrderDto> Orders);


