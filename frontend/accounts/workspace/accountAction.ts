/**
 * State shared by account server actions.
 */
interface AccountActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Navigation data shared by account server actions.
 */
interface AccountActionPayload {
  readonly redirectUrl: string;
}

export type { AccountActionPayload, AccountActionState };
