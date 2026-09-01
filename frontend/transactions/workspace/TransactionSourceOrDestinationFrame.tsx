import { Button, IconButton, Stack } from "@mui/material";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { JSX, ReactNode } from "react";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import DeleteOutline from "@mui/icons-material/DeleteOutline";

/**
 * Props for the TransactionFrame component.
 */
interface TransactionSourceOrDestinationFrameProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly headerContent?: ReactNode;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: (() => void) | null;
  readonly headerContentInline?: boolean;
  readonly color?: FrameColor;
}

/**
 * Displays a framed source or destination model inside a transaction form.
 */
const TransactionSourceOrDestinationFrame = function ({
  title,
  children,
  headerContent = null,
  onAdd = null,
  onRemove = null,
  headerContentInline = false,
  color = "info",
}: TransactionSourceOrDestinationFrameProps): JSX.Element {
  return (
    <Frame
      title={title}
      color={color}
      headerContentInline={headerContentInline}
      headerContent={
        headerContent === null && onAdd === null && onRemove === null ? null : (
          <Stack direction="row" spacing={1} alignItems="center">
            {headerContent}
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
