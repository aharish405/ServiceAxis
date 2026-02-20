import { useState, useCallback, useEffect } from 'react';
import { api } from '../services/api';

export interface RecordSummaryDto {
  id: string;
  recordNumber?: string;
  state: string;
  shortDescription?: string;
  priority?: number;
  assignedToUserId?: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export function useDynamicRecords(tableName: string, initialPage: number = 1, pageSize: number = 20) {
  const [data, setData] = useState<PagedResult<RecordSummaryDto> | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState<number>(initialPage);

  const fetchRecords = useCallback(async (currentPage: number) => {
    setIsLoading(true);
    setError(null);

    try {
      // Use the centralized api instance which handles unwrapping
      const result = await api.get<PagedResult<RecordSummaryDto>>(`/records/${tableName}`, {
        params: {
          page: currentPage,
          pageSize: pageSize
        }
      });
      
      setData(result as any); // Type cast due to interceptor unwrapping
    } catch (err: any) {
      setError(err.message || "An error occurred fetching records.");
    } finally {
      setIsLoading(false);
    }
  }, [tableName, pageSize]);

  useEffect(() => {
    if (tableName) {
      fetchRecords(page);
    }
  }, [tableName, page, fetchRecords]);

  const loadPage = (newPage: number) => {
    setPage(newPage);
  };

  const refresh = () => {
    fetchRecords(page);
  };

  return {
    data,
    isLoading,
    error,
    page,
    loadPage,
    refresh
  };
}
