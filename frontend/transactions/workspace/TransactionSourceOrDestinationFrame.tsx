import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { JSX, ReactNode } from "react";
import CollectionItemDeleteButton from "@/framework/view/CollectionItemDeleteButton";
import { Stack } from "@mui/material";

/**
 * Props for the TransactionFrame component.
 */
interface TransactionSourceOrDestinationFrameProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly headerContent?: ReactNode;
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
        headerContent === null && onRemove === null ? null : (
          <Stack direction="row" spacing={1} alignItems="center">
            {headerContent}
            {onRemove === null ? null : (
              <CollectionItemDeleteButton onClick={onRemove} />
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
