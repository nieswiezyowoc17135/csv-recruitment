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
        //rule for orderNumber
        RuleFor(x => x)
            .Must(x => x.FromWhen == null && x.ToWhen == null && !x.ClientCodes!.Any())
            .When(x => !string.IsNullOrEmpty(x.OrderNumber) )
            .WithMessage("If OrderNumber is provided, FromWhen, ToWhen and ClientCodes must be empty");

        //rules for dates
        RuleFor(x => x)
            .Must(x => string.IsNullOrEmpty(x.OrderNumber) && !x.ClientCodes!.Any())
            .When(x => x.FromWhen != null && x.ToWhen != null)
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
            .When(x => x.ClientCodes!.Count > 0)
            .WithMessage("If ClientCodes are provided, OrderNumber and Dates must be empty.");

        //Rules for all
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.OrderNumber) && x.FromWhen != null && x.ToWhen != null &&
                       x.ClientCodes!.Any())
            .When(x => !string.IsNullOrEmpty(x.OrderNumber) && x.FromWhen != null && x.ToWhen != null &&
                       x.ClientCodes!.Any())
            .WithMessage("Fill all fields if u want to pass more than 1 conditional.");
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


