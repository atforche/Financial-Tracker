"use client";

import { Button, IconButton, Menu, MenuItem, Stack } from "@mui/material";
import {
  type JSX,
  type ReactNode,
  startTransition,
  useActionState,
  useState,
} from "react";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import Link from "next/link";
import MoreVert from "@mui/icons-material/MoreVert";
import type { Route } from "next";
import type { WorkspaceActionState } from "@/framework/forms/workspaceAction";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the EntityDetailActions component.
 */
interface EntityDetailActionsProps<TDeletePayload> {
  readonly entityName: "account" | "fund";
  readonly name: string;
  readonly addTransactionHref: string;
  readonly trendsHref: Route;
  readonly deleteAction: (
    state: WorkspaceActionState,
    payload: TDeletePayload,
  ) => Promise<WorkspaceActionState>;
  readonly deletePayload: TDeletePayload;
  readonly renderUpdateForm: (onClose: () => void) => ReactNode;
}

/**
 * Shows the actions for one account or fund: adding a transaction, and a menu for the less common ones.
 */
const EntityDetailActions = function <TDeletePayload>({
  entityName,
  name,
  addTransactionHref,
  trendsHref,
  deleteAction,
  deletePayload,
  renderUpdateForm,
}: EntityDetailActionsProps<TDeletePayload>): JSX.Element {
  const canWrite = useWriteAccess();
  const [menuAnchor, setMenuAnchor] = useState<HTMLElement | null>(null);
  const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
  const [deleteState, deleteDispatch, deletePending] = useActionState(
    deleteAction,
    {},
  );
  const entityLabel = entityName === "account" ? "Account" : "Fund";
  const closeMenu = function (): void {
    setMenuAnchor(null);
  };

  return (
    <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
      {canWrite ? (
        <Button component={Link} href={addTransactionHref} variant="contained">
          Add Transaction
        </Button>
      ) : null}
      <ConfirmActionDialog
        trigger={(openDialog) => (
          <>
            <IconButton
              aria-label={`${entityLabel} actions`}
              onClick={(event) => {
                setMenuAnchor(event.currentTarget);
              }}
            >
              <MoreVert />
            </IconButton>
            <Menu
              anchorEl={menuAnchor}
              open={menuAnchor !== null}
              onClose={closeMenu}
            >
              <MenuItem component={Link} href={trendsHref}>
                Compare in Trends
              </MenuItem>
              {canWrite ? (
                <MenuItem
                  onClick={() => {
                    closeMenu();
                    setUpdateDialogOpen(true);
                  }}
                >
                  Edit
                </MenuItem>
              ) : null}
              {canWrite ? (
                <MenuItem
                  sx={{ color: "error.main" }}
                  onClick={() => {
                    closeMenu();
                    openDialog();
                  }}
                >
                  Delete
                </MenuItem>
              ) : null}
            </Menu>
          </>
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
      {canWrite && updateDialogOpen
        ? renderUpdateForm(() => {
            setUpdateDialogOpen(false);
          })
        : null}
    </Stack>
  );
};

export default EntityDetailActions;
