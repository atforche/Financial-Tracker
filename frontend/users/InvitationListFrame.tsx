"use client";

import { type JSX, useState } from "react";
import PagedTable, { type PagedTableColumn } from "@/framework/view/PagedTable";
import { formatDate, formatUserRole } from "@/users/userManagementHelpers";
import { getPaginationIndex, getRowsPerPage } from "@/framework/listframe/page";
import ManageInvitationDialog from "@/users/ManageInvitationDialog";
import type { UserInvitation } from "@/users/types";
import { useSearchParams } from "next/navigation";

/**
 * Props for the paged invitation-history list.
 */
interface InvitationListFrameProps {
  readonly invitations: readonly UserInvitation[];
}

const pageParamName = "invitationsPage";

/**
 * Displays invitation history with pending-invitation actions.
 */
const InvitationListFrame = function ({
  invitations,
}: InvitationListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const [managedInvitation, setManagedInvitation] =
    useState<UserInvitation | null>(null);
  const rowsPerPage = getRowsPerPage(searchParams.get("pageSize"));
  const paginationIndex = getPaginationIndex(
    searchParams.get(pageParamName),
    invitations.length,
    rowsPerPage,
  );
  const pageInvitations = invitations.slice(
    paginationIndex * rowsPerPage,
    (paginationIndex + 1) * rowsPerPage,
  );
  const columns: readonly PagedTableColumn<UserInvitation>[] = [
    {
      name: "email",
      header: "Email",
      getValue: (invitation) => invitation.email,
      width: "minmax(0, 1.5fr)",
    },
    {
      name: "role",
      header: "Role",
      getValue: (invitation) => formatUserRole(invitation.role),
      width: "minmax(0, 0.8fr)",
      hideOnMobile: true,
    },
    {
      name: "status",
      header: "Status",
      getValue: (invitation) => invitation.status,
      width: "minmax(0, 0.8fr)",
    },
    {
      name: "created",
      header: "Created",
      getValue: (invitation) => formatDate(invitation.createdAt),
      width: "minmax(0, 1fr)",
      hideOnMobile: true,
    },
    {
      name: "accepted",
      header: "Accepted",
      getValue: (invitation) => formatDate(invitation.acceptedAt),
      width: "minmax(0, 1fr)",
      hideOnMobile: true,
    },
  ];

  return (
    <>
      <PagedTable
        title="Invitation History"
        columns={columns}
        data={pageInvitations}
        getKey={(item) => item.id}
        onRowClick={(item) => {
          setManagedInvitation(item);
        }}
        totalCount={invitations.length}
        pageParamName={pageParamName}
        emptyState={{
          title: "No Invitations",
          description: "Create an invitation to grant a collaborator access.",
        }}
      />
      {managedInvitation !== null ? (
        <ManageInvitationDialog
          invitation={managedInvitation}
          onClose={() => {
            setManagedInvitation(null);
          }}
        />
      ) : null}
    </>
  );
};

export default InvitationListFrame;
