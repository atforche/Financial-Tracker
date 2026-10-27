"use client";

import Frame, { type FrameColor } from "@/framework/view/Frame";
import {
  type JSX,
  type ReactNode,
  createContext,
  useContext,
  useState,
} from "react";
import CollectionItemDeleteButton from "@/framework/view/CollectionItemDeleteButton";
import { Stack } from "@mui/material";

/**
 * Context exposing the header element of the enclosing source or destination frame, so that nested
 * content can render header actions into it.
 */
const FrameHeaderSlotContext = createContext<HTMLElement | null>(null);

/**
 * Gets the header element of the enclosing source or destination frame, if any.
 */
const useFrameHeaderSlot = function (): HTMLElement | null {
  return useContext(FrameHeaderSlotContext);
};

/**
 * Props for the TransactionFrame component.
 */
interface TransactionSourceOrDestinationFrameProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly headerContent?: ReactNode;
  readonly onRemove?: (() => void) | null;
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
  color = "info",
}: TransactionSourceOrDestinationFrameProps): JSX.Element {
  const [headerSlot, setHeaderSlot] = useState<HTMLElement | null>(null);
  return (
    <Frame
      title={title}
      color={color}
      headerContentInline
      headerContent={
        <Stack
          direction="row"
          spacing={1}
          alignItems="center"
          sx={{ "&:not(:has(> :not(:empty)))": { display: "none" } }}
        >
          {headerContent}
          <Stack
            ref={setHeaderSlot}
            direction="row"
            spacing={1}
            alignItems="center"
          />
          {onRemove === null ? null : (
            <CollectionItemDeleteButton onClick={onRemove} />
          )}
        </Stack>
      }
    >
      <FrameHeaderSlotContext.Provider value={headerSlot}>
        {children}
      </FrameHeaderSlotContext.Provider>
    </Frame>
  );
};

export default TransactionSourceOrDestinationFrame;
export { useFrameHeaderSlot };
