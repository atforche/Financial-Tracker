"use client";

import { useState, useTransition } from "react";
import type { UserManagementActionState } from "@/users/userManagementActions";
import { useRouter } from "next/navigation";

/**
 * Executes an administration action and refreshes server-provided page data.
 */
const useUserManagementAction = function (): {
  readonly pending: boolean;
  readonly run: (action: () => Promise<UserManagementActionState>) => void;
  readonly state: UserManagementActionState;
} {
  const router = useRouter();
  const [pending, startTransition] = useTransition();
  const [state, setState] = useState<UserManagementActionState>({
    errorTitle: null,
    unmappedErrors: null,
    success: false,
  });

  const run = function (
    action: () => Promise<UserManagementActionState>,
  ): void {
    startTransition(async () => {
      const result = await action();
      setState(result);
      if (result.success) {
        router.refresh();
      }
    });
  };

  return { pending, run, state };
};

export default useUserManagementAction;
