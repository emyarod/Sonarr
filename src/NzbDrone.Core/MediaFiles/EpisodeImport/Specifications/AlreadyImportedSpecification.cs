using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class AlreadyImportedSpecification : IImportDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AlreadyImportedSpecification(IHistoryService historyService,
                                            IConfigService configService,
                                            Logger logger)
        {
            _historyService = historyService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Skip;

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client information is available, skipping");
                return Decision.Accept();
            }

            foreach (var episode in localEpisode.Episodes)
            {
                if (!episode.HasFile)
                {
                    _logger.Debug("Skipping already imported check for episode without file");
                    continue;
                }

                var episodeHistory = _historyService.FindByEpisodeId(episode.Id);
                var lastImported = episodeHistory.FirstOrDefault(h => h.EventType == HistoryEventType.DownloadFolderImported);

                if (lastImported == null)
                {
                    continue;
                }

                // TODO: Ignore last imported check if another release was grabbed
                // See: https://github.com/Sonarr/Sonarr/issues/2393

                if (lastImported.DownloadId == downloadClientItem.DownloadId)
                {
                    _logger.Debug("Episode file previously imported");
                    return Decision.Reject("Episode file already imported");
                }
            }

            return Decision.Accept();
        }
    }
}
