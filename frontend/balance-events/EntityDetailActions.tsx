"use client";

import {
  type JSX,
  type ReactNode,
  startTransition,
  useActionState,
  useState,
} from "react";
import ActionIconButton from "@/framework/view/ActionIconButton";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import { Stack } from "@mui/material";
import type { WorkspaceActionState } from "@/framework/forms/workspaceAction";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the EntityDetailActions component.
 */
interface EntityDetailActionsProps<TDeletePayload> {
  readonly entityName: "account" | "fund";
  readonly name: string;
  readonly deleteAction: (
    state: WorkspaceActionState,
    payload: TDeletePayload,
  ) => Promise<WorkspaceActionState>;
  readonly deletePayload: TDeletePayload;
  readonly renderUpdateForm: (onClose: () => void) => ReactNode;
}

/**
 * Shows the edit and delete actions for one account or fund.
 */
const EntityDetailActions = function <TDeletePayload>({
  entityName,
  name,
  deleteAction,
  deletePayload,
  renderUpdateForm,
}: EntityDetailActionsProps<TDeletePayload>): JSX.Element | null {
  const canWrite = useWriteAccess();
  const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
  const [deleteState, deleteDispatch, deletePending] = useActionState(
    deleteAction,
    {},
  );
  const entityLabel = entityName === "account" ? "Account" : "Fund";

  if (!canWrite) {
    return null;
  }
  return (
    <Stack direction="row" spacing={0.5} sx={{ alignItems: "center" }}>
      <ActionIconButton
        action="edit"
        label={`Edit ${entityName}`}
        onClick={() => {
          setUpdateDialogOpen(true);
        }}
      />
      <ConfirmActionDialog
        trigger={(openDialog) => (
          <ActionIconButton
            action="delete"
            label={`Delete ${entityName}`}
            onClick={openDialog}
          />
        )}
        title={`Delete ${entityLabel}`}
        confirmationCopy={
          <>
            Are you sure you want to delete the {entityName} &quot;{name}&quot;?
          </>
        }
        confirmLabel="Delete"
        confirmButtonProps={{ color: "error" }}
        pending={deletePending}
        errorTitle={deleteState.errorTitle}
        unmappedErrors={deleteState.unmappedErrors}
        onConfirm={() => {
          startTransition(() => {
            deleteDispatch(deletePayload);
          });
        }}
      />
      {updateDialogOpen
        ? renderUpdateForm(() => {
            setUpdateDialogOpen(false);
          })
        : null}
    </Stack>
  );
};

export default EntityDetailActions;
