import type { JSX, ReactNode } from "react";
import DeleteOutline from "@mui/icons-material/DeleteOutline";
import Frame from "@/framework/view/Frame";
import { IconButton } from "@mui/material";

/**
 * Props for the TransactionFrame component.
 */
interface TransactionSourceOrDestinationFrameProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly onRemove?: (() => void) | null;
}

/**
 * Displays a framed source or destination model inside a transaction form.
 */
const TransactionSourceOrDestinationFrame = function ({
  title,
  children,
  onRemove = null,
}: TransactionSourceOrDestinationFrameProps): JSX.Element {
  return (
    <Frame
      title={title}
      headerContent={
        onRemove === null ? null : (
          <IconButton size="small" color="error" onClick={onRemove}>
            <DeleteOutline fontSize="small" />
          </IconButton>
        )
      }
    >
      {children}
    </Frame>
  );
};

export default TransactionSourceOrDestinationFrame;
