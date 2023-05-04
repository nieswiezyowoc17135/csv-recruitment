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
        RuleFor(x => x.OrderNumber)
            .NotNull()
            .When(x => x.FromWhen == null && x.ToWhen == null && (x.ClientCodes == null || !x.ClientCodes.Any()))
            .WithMessage("If OrderNumber is specified, all other arguments must be null or an empty list.");

        RuleFor(x => x.FromWhen)
            .NotNull()
            .When(x => x.ToWhen != null && (x.OrderNumber == null || x.ClientCodes == null || !x.ClientCodes.Any()))
            .WithMessage("If both FromWhen and ToWhen are specified, all other arguments must be null or an empty list.");

        RuleFor(x => x.ToWhen)
            .NotNull()
            .When(x => x.FromWhen != null && (x.OrderNumber == null || x.ClientCodes == null || !x.ClientCodes.Any()))
            .WithMessage("If both FromWhen and ToWhen are specified, all other arguments must be null or an empty list.");

        RuleFor(x => x.ClientCodes)
            .NotNull()
            .When(x => x.OrderNumber == null && x.FromWhen == null && x.ToWhen == null)
            .WithMessage("If ClientCodes is specified, all other arguments must be null.");

        RuleFor(x => x)
            .Must(x => x.OrderNumber != null || (x.FromWhen != null && x.ToWhen != null) || x.ClientCodes != null)
            .WithMessage("At least one of OrderNumber, FromWhen/ToWhen or ClientCodes must be specified.")
            .Must(x => x.OrderNumber == null || (x.FromWhen == null && x.ToWhen == null && (x.ClientCodes == null || !x.ClientCodes.Any())))
            .WithMessage("If OrderNumber is specified, all other arguments must be null or an empty list.")
            .Must(x => x.FromWhen == null || x.ToWhen == null || x.FromWhen <= x.ToWhen)
            .WithMessage("FromWhen must be less than or equal to ToWhen.");
        /*//rule for orderNumber
        RuleFor(x => x)
            .Must(x => x.FromWhen == null && x.ToWhen == null && !x.ClientCodes!.Any())
            .When(x => !string.IsNullOrEmpty(x.OrderNumber) && x.FromWhen == null && x.ToWhen == null && x.ClientCodes!.Count > 0)
            .WithMessage("If OrderNumber is provided, FromWhen, ToWhen and ClientCodes must be empty");

        //rules for dates
        RuleFor(x => x)
            .Must(x => string.IsNullOrEmpty(x.OrderNumber) && !x.ClientCodes!.Any())
            .When(x => x.FromWhen != null && x.ToWhen != null && string.IsNullOrEmpty(x.OrderNumber) && !x.ClientCodes!.Any())
            .WithMessage("If Dates are provided, OrderNumber and ClientCoeds must be empty.");

        RuleFor(x => x)
            .Must(x => x.FromWhen != null)
            .When(x => x.ToWhen != null)
            .WithMessage("Fill fromWhen date.");

        RuleFor(x => x)
            .Must(x => x.ToWhen != null)
            .When(x => x.FromWhen != null)
            .WithMessage("Fill toWhen date.");

        //rule for clientCodes
        RuleFor(x => x)
            .Must(x => string.IsNullOrEmpty(x.OrderNumber) && x.FromWhen == null && x.ToWhen == null)
            .When(x => x.ClientCodes!.Count > 0 && string.IsNullOrEmpty(x.OrderNumber) && x.FromWhen == null && x.ToWhen == null)
            .WithMessage("If ClientCodes are provided, OrderNumber and Dates must be empty.");

        //Rules for all
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.OrderNumber) && x.FromWhen != null && x.ToWhen != null &&
                       x.ClientCodes!.Any())
            .When(x => !string.IsNullOrEmpty(x.OrderNumber) && x.FromWhen != null && x.ToWhen != null &&
                       x.ClientCodes!.Any())
            .WithMessage("Fill all fields if u want to pass more than 1 conditional.");*/
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
            var temp = result.Errors;
            if (!result.IsValid)
            {
                throw new Exception("Something is wrong with passed arguments.");
            }
            foreach (var error in temp)
            {
                var temp1 =error.ErrorMessage;
            }

            using var streamReaderNew = new StreamReader(_config["FilePath"]);
            using var csvReader = new CsvReader(streamReaderNew, CultureInfo.InvariantCulture);
            //probably not necessary
            //csvReader.Context.TypeConverterCache.AddConverter<DateTime>(new DateTimeConverter());
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
                results = records.Where(x => x.OrderDate > request.FromWhen && x.ShipmentDate <= request.ToWhen).ToList();
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


