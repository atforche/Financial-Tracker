"use client";

import { Button, MenuItem, Stack, TextField, Typography } from "@mui/material";
import { type JSX, useState, useTransition } from "react";
import {
  consolidateLocation,
  deleteLocation,
  renameLocation,
} from "@/locations/actions";
import Dialog from "@/framework/dialog/Dialog";
import type { Location } from "@/locations/types";
import { useRouter } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the Location Detail Actions component.
 */
interface LocationDetailActionsProps {
  readonly location: Location;
  readonly locations: readonly Location[];
  readonly workspaceUrl: string;
}

/** Provides Location maintenance actions through focused dialogs. */
const LocationDetailActions = function ({
  location,
  locations,
  workspaceUrl,
}: LocationDetailActionsProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const router = useRouter();
  const [dialog, setDialog] = useState<
    "rename" | "consolidate" | "delete" | null
  >(null);
  const [name, setName] = useState(location.name);
  const [targetLocationId, setTargetLocationId] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const closeDialog = function (): void {
    if (!pending) {
      setDialog(null);
      setError(null);
    }
  };
  const runAction = function (
    action: (formData: FormData) => Promise<void>,
    formData: FormData,
    redirect = false,
  ): void {
    startTransition(async () => {
      try {
        await action(formData);
        if (redirect) {
          router.push(workspaceUrl);
        } else {
          router.refresh();
          setDialog(null);
        }
      } catch (caughtError) {
        setError(
          caughtError instanceof Error ? caughtError.message : "Action failed.",
        );
      }
    });
  };

  if (!canWrite) {
    return null;
  }

  return (
    <>
      <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
        <Button
          variant="contained"
          onClick={() => {
            setDialog("rename");
          }}
        >
          Rename
        </Button>
        <Button
          variant="outlined"
          onClick={() => {
            setDialog("consolidate");
          }}
        >
          Consolidate
        </Button>
        <Button
          color="error"
          variant="outlined"
          onClick={() => {
            setDialog("delete");
          }}
        >
          Delete
        </Button>
      </Stack>
      <Dialog
        open={dialog === "rename"}
        onClose={closeDialog}
        title="Rename Location"
        fullWidth
        maxWidth="sm"
        actions={
          <>
            <Button disabled={pending} onClick={closeDialog}>
              Cancel
            </Button>
            <Button
              variant="contained"
              loading={pending}
              onClick={() => {
                const formData = new FormData();
                formData.set("id", location.id);
                formData.set("name", name);
                runAction(renameLocation, formData);
              }}
            >
              Rename
            </Button>
          </>
        }
      >
        <Stack spacing={2}>
          <TextField
            label="Name"
            value={name}
            onChange={(event) => {
              setName(event.target.value);
            }}
            autoFocus
          />
          {error === null ? null : (
            <Typography color="error">{error}</Typography>
          )}
        </Stack>
      </Dialog>
      <Dialog
        open={dialog === "consolidate"}
        onClose={closeDialog}
        title="Consolidate Location"
        fullWidth
        maxWidth="sm"
        actions={
          <>
            <Button disabled={pending} onClick={closeDialog}>
              Cancel
            </Button>
            <Button
              variant="contained"
              disabled={targetLocationId === ""}
              loading={pending}
              onClick={() => {
                const formData = new FormData();
                formData.set("sourceId", location.id);
                formData.set("targetLocationId", targetLocationId);
                runAction(consolidateLocation, formData, true);
              }}
            >
              Consolidate
            </Button>
          </>
        }
      >
        <Stack spacing={2}>
          <Typography>
            Move all transactions from {location.name} to the selected Location.
          </Typography>
          <TextField
            select
            label="Keep Location"
            value={targetLocationId}
            onChange={(event) => {
              setTargetLocationId(event.target.value);
            }}
          >
            {locations
              .filter((candidate) => candidate.id !== location.id)
              .map((candidate) => (
                <MenuItem key={candidate.id} value={candidate.id}>
                  {candidate.name}
                </MenuItem>
              ))}
          </TextField>
          {error === null ? null : (
            <Typography color="error">{error}</Typography>
          )}
        </Stack>
      </Dialog>
      <Dialog
        open={dialog === "delete"}
        onClose={closeDialog}
        title="Delete Location"
        fullWidth
        maxWidth="sm"
        actions={
          <>
            <Button disabled={pending} onClick={closeDialog}>
              Cancel
            </Button>
            <Button
              color="error"
              variant="contained"
              loading={pending}
              onClick={() => {
                const formData = new FormData();
                formData.set("id", location.id);
                runAction(deleteLocation, formData, true);
              }}
            >
              Delete
            </Button>
          </>
        }
      >
        <Stack spacing={1}>
          <Typography>Delete {location.name}?</Typography>
          <Typography variant="body2" color="text.secondary">
            Deletion is unavailable when any transaction uses this Location.
          </Typography>
          {error === null ? null : (
            <Typography color="error">{error}</Typography>
          )}
        </Stack>
      </Dialog>
    </>
  );
};

export default LocationDetailActions;
