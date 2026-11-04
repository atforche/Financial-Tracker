/**
 * Where an account or fund ended an accounting period relative to its goal.
 */
enum PeriodEntityStatus {
  BelowMinimum = "BelowMinimum",
  AboveMaximum = "AboveMaximum",
  WithinRange = "WithinRange",
  NoGoal = "NoGoal",
}

export default PeriodEntityStatus;
