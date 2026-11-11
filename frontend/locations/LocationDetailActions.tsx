"use client";

import {
  Autocomplete,
  Divider,
  IconButton,
  Menu,
  MenuItem,
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
import FormDialog from "@/framework/dialog/FormDialog";
import type { Location } from "@/locations/types";
import MoreVert from "@mui/icons-material/MoreVert";
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
  const [menuAnchor, setMenuAnchor] = useState<HTMLElement | null>(null);
  const [name, setName] = useState(location.name);
  const [mergeOptions, setMergeOptions] = useState<Location[] | null>(null);
  const [mergeTarget, setMergeTarget] = useState<Location | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();
  // Loading merge options runs apart from the actions, so the dialog stays usable while they load
  const [, startLoadingOptions] = useTransition();

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
      startLoadingOptions(async () => {
        try {
          const options = await getLocationOptions();
          setMergeOptions(
            options.filter((candidate) => candidate.id !== location.id),
          );
        } catch {
          setMergeOptions([]);
          setError("Unable to load the locations to merge into.");
        }
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
      <IconButton
        aria-label="Location actions"
        onClick={(event) => {
          setMenuAnchor(event.currentTarget);
        }}
      >
        <MoreVert />
      </IconButton>
      <Menu
        anchorEl={menuAnchor}
        open={menuAnchor !== null}
        onClose={() => {
          setMenuAnchor(null);
        }}
      >
        <MenuItem
          onClick={() => {
            setMenuAnchor(null);
            setDialog("rename");
          }}
        >
          Rename
        </MenuItem>
        <MenuItem
          onClick={() => {
            setMenuAnchor(null);
            openMerge();
          }}
        >
          Merge
        </MenuItem>
        <Divider />
        <MenuItem
          sx={{ color: "error.main" }}
          onClick={() => {
            setMenuAnchor(null);
            setDialog("delete");
          }}
        >
          Delete
        </MenuItem>
      </Menu>
      <FormDialog
        open={dialog === "rename"}
        title="Rename Location"
        onClose={closeDialog}
        pending={pending}
        submitLabel="Rename"
        onSubmit={() => {
          runAction(async () => renameLocation(location.id, name), false);
        }}
        canSubmit={name.trim() !== ""}
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
      </FormDialog>
      <FormDialog
        open={dialog === "merge"}
        title="Merge Location"
        onClose={closeDialog}
        pending={pending}
        submitLabel="Merge"
        onSubmit={() => {
          if (mergeTarget !== null) {
            runAction(
              async () => mergeLocation(location.id, mergeTarget.id),
              true,
            );
          }
        }}
        canSubmit={mergeTarget !== null}
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
      </FormDialog>
      <FormDialog
        open={dialog === "delete"}
        title="Delete Location"
        onClose={closeDialog}
        pending={pending}
        submitLabel="Delete"
        onSubmit={() => {
          runAction(async () => deleteLocation(location.id), true);
        }}
        submitButtonProps={{ color: "error" }}
      >
        <Stack spacing={1}>
          <Typography>Delete {location.name}?</Typography>
          <Typography variant="body2" color="text.secondary">
            A location can only be deleted when no transaction uses it. To
            remove one that is in use, merge it into another location.
          </Typography>
          {errorMessage}
        </Stack>
      </FormDialog>
    </>
  );
};

export default LocationDetailActions;
