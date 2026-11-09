"use client";

import {
  Autocomplete,
  Button,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { type JSX, useState, useTransition } from "react";
import {
  deleteLocation,
  getLocationOptions,
  mergeLocation,
  renameLocation,
} from "@/locations/actions";
import Dialog from "@/framework/dialog/Dialog";
import type { Location } from "@/locations/types";
import { useRouter } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the LocationDetailActions component.
 */
interface LocationDetailActionsProps {
  readonly location: Location;
  readonly workspaceUrl: string;
}

/**
 * Renames, merges, or deletes a location from its page.
 */
const LocationDetailActions = function ({
  location,
  workspaceUrl,
}: LocationDetailActionsProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const router = useRouter();
  const [dialog, setDialog] = useState<"rename" | "merge" | "delete" | null>(
    null,
  );
  const [name, setName] = useState(location.name);
  const [mergeOptions, setMergeOptions] = useState<Location[] | null>(null);
  const [mergeTarget, setMergeTarget] = useState<Location | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const closeDialog = function (): void {
    if (!pending) {
      setDialog(null);
      setError(null);
    }
  };

  /**
   * Runs an action, then leaves the page when the location is gone or refreshes it when it was renamed.
   */
  const runAction = function (
    action: () => Promise<string | null>,
    leavesPage: boolean,
  ): void {
    startTransition(async () => {
      const failure = await action();
      if (failure !== null) {
        setError(failure);
      } else if (leavesPage) {
        router.push(workspaceUrl);
      } else {
        setDialog(null);
        router.refresh();
      }
    });
  };

  const openMerge = function (): void {
    setDialog("merge");
    if (mergeOptions === null) {
      startTransition(async () => {
        const options = await getLocationOptions();
        setMergeOptions(
          options.filter((candidate) => candidate.id !== location.id),
        );
      });
    }
  };

  if (!canWrite) {
    return null;
  }

  const errorMessage =
    error === null ? null : <Typography color="error">{error}</Typography>;

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
        <Button variant="outlined" onClick={openMerge}>
          Merge
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
              disabled={name.trim() === ""}
              onClick={() => {
                runAction(async () => renameLocation(location.id, name), false);
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
          {errorMessage}
        </Stack>
      </Dialog>
      <Dialog
        open={dialog === "merge"}
        onClose={closeDialog}
        title="Merge Location"
        fullWidth
        maxWidth="sm"
        actions={
          <>
            <Button disabled={pending} onClick={closeDialog}>
              Cancel
            </Button>
            <Button
              variant="contained"
              disabled={mergeTarget === null}
              loading={pending}
              onClick={() => {
                if (mergeTarget !== null) {
                  runAction(
                    async () => mergeLocation(location.id, mergeTarget.id),
                    true,
                  );
                }
              }}
            >
              Merge
            </Button>
          </>
        }
      >
        <Stack spacing={2}>
          <Typography>
            Moves every transaction at {location.name} to the location you
            choose, then removes {location.name}.
          </Typography>
          <Autocomplete
            options={mergeOptions ?? []}
            loading={mergeOptions === null}
            value={mergeTarget}
            onChange={(_, value) => {
              setMergeTarget(value);
            }}
            getOptionLabel={(option) => option.name}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            renderInput={(params) => (
              <TextField {...params} label="Merge into" autoFocus />
            )}
          />
          {errorMessage}
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
                runAction(async () => deleteLocation(location.id), true);
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
            A location can only be deleted when no transaction uses it. To
            remove one that is in use, merge it into another location.
          </Typography>
          {errorMessage}
        </Stack>
      </Dialog>
    </>
  );
};

export default LocationDetailActions;
