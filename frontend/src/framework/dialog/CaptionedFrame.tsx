import type { JSX } from "react";
import { Typography } from "@mui/material";

const defaultMinWidth = 300;

/**
 * Props for CaptionedFrame component.
 */
interface CaptionedFrameProps {
  readonly caption: string;
  readonly minWidth?: number;
  readonly children: React.ReactNode;
}

/**
 * Component that presents the user with a frame that has a caption and children content.
 * @param props - Props for the CaptionedFrame component.
 * @returns JSX element representing the CaptionedFrame.
 */
const CaptionedFrame = function ({
  caption,
  minWidth = defaultMinWidth,
  children,
}: CaptionedFrameProps): JSX.Element {
  return (
    <fieldset
      style={{
        border: "1px solid rgba(0, 0, 0, 0.23)",
        borderRadius: "5px",
        padding: "0px 15px 15px 15px",
        minWidth,
      }}
    >
      <legend>
        <Typography variant="caption" sx={{ color: "rgba(0, 0, 0, 0.6)" }}>
          {caption}
        </Typography>
      </legend>
      {children}
    </fieldset>
  );
};

export default CaptionedFrame;
