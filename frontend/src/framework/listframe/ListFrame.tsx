import "@framework/listframe/ListFrame.css";
import {
  Box,
  Paper,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  Typography,
} from "@mui/material";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import type { JSX } from "react";

/** Height of each row in the list frame. */
const listFrameRowHeight = 50;

/** Options for the number of rows per page in the list frame. */
const rowsPerPageOptions = [5, 10, 25];

/**
 * Props for the ListFrame component.
 */
interface ListFrameProps<T> {
  readonly name: string;
  readonly columns: ColumnDefinition<T>[];
  readonly getId: (item: T) => string;
  readonly data: T[] | null;
  readonly totalCount: number | null;
  readonly isLoading: boolean;
  readonly isError: boolean;
  readonly page: number;
  readonly setPage: (page: number) => void;
  readonly rowsPerPage: number;
  readonly setRowsPerPage: (rowsPerPage: number) => void;
  readonly children: React.ReactNode;
}

/**
 * Component that presents a generic list frame with a table structure.
 * @template T - Type of the data items to be displayed in the list frame.
 * @param props - Props for the ListFrame component.
 * @returns JSX element representing the ListFrame.
 */
const ListFrame = function <T>({
  name,
  columns,
  getId,
  data,
  totalCount,
  isLoading,
  isError,
  page,
  setPage,
  rowsPerPage,
  setRowsPerPage,
  children,
}: ListFrameProps<T>): JSX.Element {
  return (
    <Box>
      <Typography variant="h6" sx={{ paddingBottom: "25px" }}>
        {name}
      </Typography>
      <Paper
        sx={{
          width: "100%",
        }}
      >
        <TableContainer>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                {columns.map((column) => (
                  <ColumnHeader key={column.name} column={column} />
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.map((item) => (
                <TableRow hover tabIndex={-1} key={getId(item)}>
                  {columns.map((column) => (
                    <TableCell
                      className="list-frame-table-cell"
                      key={`${getId(item)}-${column.name}`}
                      align={column.alignment ?? "left"}
                      sx={{
                        paddingTop: "8px",
                        paddingBottom: "8px",
                      }}
                    >
                      {column.getBodyContent(item)}
                    </TableCell>
                  ))}
                </TableRow>
              ))}
              {(data?.length ?? 0) < rowsPerPage &&
                Array(rowsPerPage - (data?.length ?? 0))
                  .fill(null)
                  .map((_, index) => (
                    <TableRow
                      style={{ height: listFrameRowHeight }}
                      key={index}
                    >
                      {Array(columns.length)
                        .fill(null)
                        .map((__, cellIndex) => (
                          <TableCell
                            className="list-frame-table-cell"
                            key={`skeleton-${index}-${cellIndex}`}
                          >
                            {isLoading ? (
                              <Skeleton />
                            ) : isError ? (
                              <Skeleton animation={false} />
                            ) : null}
                          </TableCell>
                        ))}
                    </TableRow>
                  ))}
            </TableBody>
          </Table>
        </TableContainer>
        <TablePagination
          rowsPerPageOptions={rowsPerPageOptions}
          component="div"
          count={totalCount ?? 0}
          rowsPerPage={rowsPerPage}
          page={page}
          onPageChange={(_, newPage) => {
            setPage(newPage);
          }}
          onRowsPerPageChange={(event) => {
            setRowsPerPage(parseInt(event.target.value, 10));
          }}
        />
      </Paper>
      {children}
    </Box>
  );
};

export default ListFrame;
