import { Button, IconButton, Stack } from "@mui/material";
import type { JSX, ReactNode } from "react";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import DeleteOutline from "@mui/icons-material/DeleteOutline";
import Frame from "@/framework/view/Frame";

/**
 * Props for the TransactionFrame component.
 */
interface TransactionSourceOrDestinationFrameProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: (() => void) | null;
}

/**
 * Displays a framed source or destination model inside a transaction form.
 */
const TransactionSourceOrDestinationFrame = function ({
  title,
  children,
  onAdd = null,
  onRemove = null,
}: TransactionSourceOrDestinationFrameProps): JSX.Element {
  return (
    <Frame
      title={title}
      headerContent={
        onAdd === null && onRemove === null ? null : (
          <Stack direction="row" spacing={1} alignItems="center">
            {onAdd === null ? null : (
              <Button
                variant="outlined"
                size="small"
                startIcon={<AddCircleOutline />}
                onClick={onAdd}
              >
                Add Destination
              </Button>
            )}
            {onRemove === null ? null : (
              <IconButton size="small" color="error" onClick={onRemove}>
                <DeleteOutline fontSize="small" />
              </IconButton>
            )}
          </Stack>
        )
      }
    >
      {children}
    </Frame>
  );
};

export default TransactionSourceOrDestinationFrame;
