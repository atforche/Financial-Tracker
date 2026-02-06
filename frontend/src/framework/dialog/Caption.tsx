import type { JSX } from "react";
import { Typography } from "@mui/material";

/**
 * Props for the Caption component.
 */
interface CaptionProps {
  readonly caption: string;
}

/**
 * Component that presents the user with a caption.
 * @param props - Props for the Caption component.
 * @returns JSX element representing the Caption component.
 */
const Caption = function ({ caption }: CaptionProps): JSX.Element {
  return (
    <Typography variant="subtitle1">
      <b>{caption}:</b>
    </Typography>
  );
};

export default Caption;
