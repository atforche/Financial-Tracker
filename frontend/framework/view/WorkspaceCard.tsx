import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { JSX, ReactNode } from "react";
import { ButtonBase } from "@mui/material";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";

/**
 * Props for the WorkspaceCard component.
 */
interface WorkspaceCardContentProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly color?: FrameColor;
}

/**
 * Props for the WorkspaceCard component, which can either include an href for navigation or an onClick handler for interaction.
 */
type WorkspaceCardProps = WorkspaceCardContentProps &
  (
    | { readonly href: string; readonly onClick?: never }
    | { readonly href?: never; readonly onClick: () => void }
  );

/**
 * Styles for the button base used in the WorkspaceCard component, ensuring consistent layout and appearance.
 */
const buttonSx = {
  display: "flex",
  width: "100%",
  minWidth: 0,
  borderRadius: 5,
  textAlign: "left",
  "& .MuiPaper-root": { width: "100%" },
} as const;

/**
 * Displays a consistently styled, navigable card in a workspace grid.
 */
const WorkspaceCard = function ({
  title,
  children,
  color,
  href,
  onClick,
}: WorkspaceCardProps): JSX.Element {
  const content = (
    <Frame
      title={title}
      {...(color === undefined ? {} : { color })}
      headerContentInline
      headerContent={
        <KeyboardArrowRight sx={{ color: "text.secondary", fontSize: 22 }} />
      }
    >
      {children}
    </Frame>
  );

  return href === undefined ? (
    <ButtonBase onClick={onClick} sx={buttonSx}>
      {content}
    </ButtonBase>
  ) : (
    <ButtonBase href={href} sx={buttonSx}>
      {content}
    </ButtonBase>
  );
};

export type { WorkspaceCardProps };
export default WorkspaceCard;
