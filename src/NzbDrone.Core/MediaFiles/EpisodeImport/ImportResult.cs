using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public class ImportResult
    {
        public ImportDecision ImportDecision { get; private set; }
        public List<string> Errors { get; private set; }

        public ImportResultType Result
        {
            get
            {
                if (Errors.Any())
                {
                    if (ImportDecision.Approved)
                    {
                        return ImportResultType.Skipped;
                    }

                    return ImportResultType.Rejected;
                }

                if (ImportDecision.Rejections.All(r => r.Type == RejectionType.Skip))
                {
                    return ImportResultType.Skipped;
                }

                return ImportResultType.Imported;
            }
        }

        public ImportResult(ImportDecision importDecision, params string[] errors)
        {
            Ensure.That(importDecision, () => importDecision).IsNotNull();

            ImportDecision = importDecision;
            Errors = errors.ToList();
        }
    }
}
