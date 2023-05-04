using System.Globalization;
using csv_project.Data.Entries;
using CsvHelper;
using FluentValidation;
using MediatR;

namespace csv_project.Features.Query;

public record CsvReadQuery(string? OrderNumber, DateTime? FromWhen, DateTime? ToWhen, List<string>? ClientCodes) : IRequest<CsvReadQueryResult>;

public class CsvReadQueryValidator : AbstractValidator<CsvReadQuery>
{
    public CsvReadQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => x.ClientCodes!.Count == 0 && x.OrderNumber == null)
            .When(x => x.FromWhen != null || x.ToWhen != null)
            .WithMessage("If Dates are passed, other arguments have to be null.");

        RuleFor(x => x)
            .Must(x => x.OrderNumber == null && x.FromWhen == null && x.ToWhen == null)
            .When(x => x.ClientCodes!.Count > 0)
            .WithMessage("If ClientCodes are passed, other arguments have to be null.");

        RuleFor(x => x)
            .Must(x => x.ClientCodes!.Count == 0 && x.FromWhen == null & x.ToWhen == null)
            .When(x => x.OrderNumber != null)
            .WithMessage("If Order Number is passed, other arguments have to be null.");

        RuleFor(x => x.FromWhen)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .When(x => x.OrderNumber == null && x.ClientCodes!.Count == 0 && x.ToWhen != null)
            .WithMessage("FromWhen must not be null if ToWhen is not null and other arguments are null.");

        RuleFor(x => x.ToWhen)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .When(x => x.OrderNumber == null && x.ClientCodes!.Count ==0 && x.FromWhen != null)
            .WithMessage("ToWhen must not be null if FromWhen is not null and other arguments are null.");

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x => x.OrderNumber != null || x.ClientCodes!.Count > 0 || (x.FromWhen != null && x.ToWhen != null))
            .WithMessage("At least one of OrderNumber, ClientCodes, or FromWhen/ToWhen must be specified.");

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x => x.FromWhen == null || x.ToWhen == null || x.FromWhen <= x.ToWhen)
            .WithMessage("FromWhen must be less than or equal to ToWhen.");
    }
}

public class CsvReaderClass
{
    public class CsvReaderQueryHandler : IRequestHandler<CsvReadQuery,CsvReadQueryResult>
    {
        private readonly IConfiguration _config;
        public CsvReaderQueryHandler(IConfiguration config)
        {
            _config = config;
        }
        public Task<CsvReadQueryResult> Handle(CsvReadQuery request, CancellationToken cancellationToken)
        {
            //validation
            var validator = new CsvReadQueryValidator();
            var result = validator.Validate(request);
            if (!result.IsValid)
            {
                throw new Exception("Something is wrong with passed arguments.");
            }

            using var streamReaderNew = new StreamReader(_config["FilePath"]);
            using var csvReader = new CsvReader(streamReaderNew, CultureInfo.InvariantCulture);
            csvReader.Context.RegisterClassMap<OrderEntryMap>();
            var records = csvReader.GetRecords<OrderEntry>().ToList();
            IEnumerable<OrderEntry> results = new List<OrderEntry>();

            //case for order
            if (!string.IsNullOrEmpty(request.OrderNumber) && (request.FromWhen is null && request.ToWhen is null) &&
                request.ClientCodes!.Count==0)
            {
                results = records.Where(x => x.Number == request.OrderNumber).ToList();
            }

            //case for dates
            if (string.IsNullOrEmpty(request.OrderNumber) &&
                (request.FromWhen is not null && request.ToWhen is not null) &&
                request.ClientCodes!.Count == 0)
            {
                results = records.Where(x => x.OrderDate > request.FromWhen && x.OrderDate <= request.ToWhen).ToList();
            }

            //case for client codes
            if (string.IsNullOrEmpty(request.OrderNumber) &&
                (request.FromWhen is null && request.ToWhen is null) &&
                request.ClientCodes!.Count > 0)
            {
                results = records.Where(x => request.ClientCodes.Contains(x.ClientCode)).ToList();
            }

            return Task.FromResult(new CsvReadQueryResult(results));
        }
    }
}

public record CsvReadQueryResult(IEnumerable<OrderEntry> Orders);


