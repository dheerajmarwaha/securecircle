using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Arya.Core.Entities;
using Arya.Core.Exceptions;
using Arya.Core.QueryModels;
using Arya.Core.Services;
using Arya.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/* @author Dheeraj Marwaha */

namespace Arya.Core.HttpClients {
    /// <summary>
    /// Methods to invoke CaRe APIs
    /// </summary>
    public class CareHttpClient {

        private readonly HttpClient _httpClient;
        private readonly ISystemConfigurationService _systemConfigurationService;
        private readonly ILogger<CareHttpClient> _logger;
        private readonly int HttpStatusCode_UnprocessableEntity = 422;

        public CareHttpClient(ILogger<CareHttpClient> logger, HttpClient httpClient, ISystemConfigurationService systemConfigurationService) {
            _logger = logger;
            _systemConfigurationService = systemConfigurationService;
            _httpClient = httpClient;

        }

        /// <summary>
        /// Method to invoke distribution stats for Global Insight
        /// </summary>
        /// <param name="query">Filters for Global Insight</param>
        /// <returns></returns>
        public virtual async Task<CandidateDistributionStatsModel> GetStatsAsync(CandidateDistributionQuery query) {
            if (_httpClient.BaseAddress == null) {
                _logger.LogError("Care API Base address must be specified. Check the setting {@Url} in system configuration", "url.care.base");
                throw new ConfigurationErrorsException("Care API base address must be specified. Check the setting - url.care.base in system configuration");
            }

            try {
                var request = new {
                    title = query.Title,
                    titleSynonyms = query.TitleSynonyms,
                    companies = query.Companies,
                    industries = query.Industries,
                    skills = query.Skills,

                    isSimilarTitlesStats = true,
                    isOccupationsStats = false,
                    isCompaniesStats = true,
                    isIndustriesStats = true,
                    isSkillsStats = true,
                    isTotalCandidateCount = true,

                    location = new {
                        countryCode = query.CountryCode,
                        stateName = query.State,
                        cityName = query.City
                    },

                    similarTitlesCount = query.TitlesCount,
                    companiesCount = query.CompaniesCount,
                    industriesCount = query.IndustriesCount,
                    skillsCount = query.SkillsCount,
                };
                var payload = JsonConvert.SerializeObject(request);
                _logger.LogInformation("CareHttpClient : Request of Get Candidate stats : {@Payload}", payload);

                var careApiDistributionRelativeUrl = await _systemConfigurationService.GetValueAsync("url.care.distribution.relativepath");
                if (careApiDistributionRelativeUrl == null) {
                    _logger.LogError("Care API distribution  relative path must be specified. Check the setting {@Url} in system configuration", "url.care.distribution.relativepath");
                    throw new ConfigurationErrorsException("Care API distribution  relative path must be specified. Check the setting - url.care.distribution.relativepath in system configuration");
                }
                var response = await _httpClient.PostAsync(careApiDistributionRelativeUrl, new StringContent(payload, Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if ((int) response.StatusCode == HttpStatusCode_UnprocessableEntity) {
                    throw new UnprocessableEntityException("CountryCode", "[CountryCode] - Country not supported");
                } else if (response.StatusCode == HttpStatusCode.NotFound) {
                    throw new EntityNotFoundException(nameof(CareHttpClient), content);
                } else if (response.StatusCode == HttpStatusCode.GatewayTimeout ||
                               response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                               response.StatusCode == HttpStatusCode.InternalServerError) {
                    throw new ServiceUnavailableException(nameof(CareHttpClient), content);
                }

                var statsModel = JsonConvert.DeserializeObject<CandidateDistributionStatsModel>(content);
                return statsModel;

            } catch (HttpRequestException ex) {
                var errMsg = "Error occurred while fetching candidate distribution stats";
                _logger.LogError(ex, errMsg);
                throw new ServiceUnavailableException(nameof(CareHttpClient), nameof(CareHttpClient.GetStatsAsync), errMsg, ex);

            }
        }

        /// <summary>
        /// Method to invoke Career progression API
        /// </summary>
        /// <param name="criteria">Filters with career profile</param>
        /// <returns></returns>
        public virtual async Task<CareerProjectionStatsModel> GetProgressionAsync(CareerProjectionQuery criteria) {
            if (_httpClient.BaseAddress == null) {
                _logger.LogError("Care API Base Url must be specified. Check the setting {@Url} in system configuration", "url.care.base");
                throw new ConfigurationErrorsException("Care API base address must be specified. Check the setting - url.care.base in system configuration");
            }

            try {
                var request = new {
                    title = criteria.Title,
                    similar_titles = criteria.SimilarTitles,
                    skills = criteria.Skills
                };
                var payload = JsonConvert.SerializeObject(request);
                _logger.LogInformation("CareHttpClient : Request of GetProgression : {@Payload}", payload);

                var careProgressionApiRelativeUrl = await _systemConfigurationService.GetValueAsync("url.care.progression.relativepath");
                if (careProgressionApiRelativeUrl == null) {
                    _logger.LogError("Care API Progression relative path must be specified. Check the setting {@Url} in system configuration", "url.care.progression.relativepath");
                    throw new ConfigurationErrorsException("Care API Progression relative path must be specified. Check the setting - url.care.progression.relativepath in system configuration");
                }
                var response = await _httpClient.PostAsync(careProgressionApiRelativeUrl, new StringContent(payload, Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.NotFound) {
                    throw new EntityNotFoundException(nameof(CareHttpClient), content);
                } else if (response.StatusCode == HttpStatusCode.GatewayTimeout ||
                   response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                   response.StatusCode == HttpStatusCode.InternalServerError) {
                    throw new ServiceUnavailableException(nameof(CareHttpClient), content);
                }
                var progressionModel = JsonConvert.DeserializeObject<CareerProjectionStatsModel>(content);
                return progressionModel;
            } catch (HttpRequestException ex) {

                var errMsg = "Error occurred while fetching career progression stats";
                _logger.LogError(ex, errMsg);
                throw new ServiceUnavailableException(nameof(CareHttpClient), nameof(CareHttpClient.GetProgressionAsync), errMsg, ex);
            }
        }

    }
}
