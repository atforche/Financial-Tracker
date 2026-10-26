/**
 * State shared by workspace server actions.
 */
interface WorkspaceActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Navigation data shared by workspace server actions.
 */
interface WorkspaceActionPayload {
  readonly redirectUrl: string;
}

export type { WorkspaceActionPayload, WorkspaceActionState };
