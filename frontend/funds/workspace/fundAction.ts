/**
 * State shared by fund server actions.
 */
interface FundActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Navigation data shared by fund server actions.
 */
interface FundActionPayload {
  readonly redirectUrl: string;
}

export type { FundActionPayload, FundActionState };
