using System.Collections.Generic;
using System.Linq;

namespace Antiban
{
    public class Antiban
    {
        private readonly List<EventMessage> _messages = new();

        public void PushEventMessage(EventMessage eventMessage)
        {
            _messages.Add(eventMessage);
        }

        public List<AntibanResult> GetResult()
        {
            var antibans = new List<AntibanResult>();
            var sameAntibans = new Dictionary<string, List<AntibanResult>>();
            var highPriorityAntibans = new Dictionary<string, List<AntibanResult>>();

            foreach (var msg in _messages)
            {
                var antiban = new AntibanResult
                {
                    EventMessageId = msg.Id,
                    SentDateTime = msg.DateTime,
                };

                if (!sameAntibans.TryGetValue(msg.Phone, out var sameList))
                {
                    sameList = new List<AntibanResult>();
                    sameAntibans.Add(msg.Phone, sameList);
                }
                sameList.Add(antiban);

                if (msg.Priority == 1)
                {
                    if (!highPriorityAntibans.TryGetValue(msg.Phone, out var highList))
                    {
                        highList = new List<AntibanResult>();
                        highPriorityAntibans.Add(msg.Phone, highList);
                    }
                    highList.Add(antiban);
                }

                antibans.Add(antiban);
            }

            foreach(var _ in Enumerable.Range(1, AntibanSettings.NumberOfNormalizationAttempts))
            {
                var isNormalized = true;

                highPriorityAntibans = highPriorityAntibans.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.OrderBy(i => i.SentDateTime).ToList()
                );

                foreach (var items in highPriorityAntibans.Values)
                    NormalizeSec(items, AntibanSettings.MinimumSecondsBetweenHighPriorityMessagesSameNumber);

                sameAntibans = sameAntibans.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.OrderBy(i => i.SentDateTime).ToList()
                );

                foreach (var items in sameAntibans.Values)
                    NormalizeSec(items, AntibanSettings.MinimumSecondsBetweenMessagesSameNumber);

                antibans = antibans.OrderBy(i => i.SentDateTime).ToList();
                NormalizeSec(antibans, AntibanSettings.MinimumSecondsBetweenAnyMessages);

                foreach (var items in sameAntibans.Values)
                    if (!IsNormalized(items, AntibanSettings.MinimumSecondsBetweenMessagesSameNumber))
                        isNormalized = false;

                foreach (var items in highPriorityAntibans.Values)
                    if (!IsNormalized(items, AntibanSettings.MinimumSecondsBetweenHighPriorityMessagesSameNumber))
                        isNormalized = false;

                if (!IsNormalized(antibans, AntibanSettings.MinimumSecondsBetweenAnyMessages))
                    isNormalized = false;

                if (isNormalized)
                    break;
            }

            return antibans;
        }

        private void NormalizeSec(List<AntibanResult> antibans, int seconds)
        {
            for(int i = 0; i < antibans.Count; i++)
            {
                if (i == 0) continue;

                var previous = antibans[i - 1];
                var current = antibans[i];

                if (current.SentDateTime < previous.SentDateTime.AddSeconds(seconds))
                    current.SentDateTime = previous.SentDateTime.AddSeconds(seconds);
            }
        }

        private bool IsNormalized(List<AntibanResult> antibans, int seconds)
        {
            for(int i = 0; i < antibans.Count; i++)
            {
                if (i == 0) continue;

                var previous = antibans[i - 1];
                var current = antibans[i];

                if (current.SentDateTime < previous.SentDateTime.AddSeconds(seconds))
                    return false;
            }

            return true;
        }

    }
}
