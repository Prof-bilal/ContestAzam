using Xunit;

// Auth flows share process-global state (rate-limiter env override, Identity lockout
// counters keyed by email). Running sequentially keeps these deterministic.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
