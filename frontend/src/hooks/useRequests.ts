import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { requestsApi } from '@/api/requests'
import { useToast } from '@/store/uiStore'
import { parseApiError } from '@/api/client'
import type { RequestFilters } from '@/types/request'

/**
 * Hooks para operações sobre pedidos.
 * Centralizam a invalidação de cache e mensagens de feedback,
 * mantendo as páginas mais limpas.
 */

export function useRequestsList(filters: RequestFilters) {
  return useQuery({
    queryKey: ['requests', filters],
    queryFn: () => requestsApi.list(filters),
  })
}

export function useRequestDetail(id: string | undefined) {
  return useQuery({
    queryKey: ['requests', id],
    queryFn: () => requestsApi.get(id!),
    enabled: !!id,
  })
}

export function useSubmitRequest(requestId: string) {
  const qc = useQueryClient()
  const toast = useToast()

  return useMutation({
    mutationFn: () => requestsApi.submit(requestId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['requests'] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      toast.success('Pedido submetido!', 'O aprovador será notificado.')
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })
}

export function useApproveRequest(requestId: string) {
  const qc = useQueryClient()
  const toast = useToast()

  return useMutation({
    mutationFn: (comment?: string) => requestsApi.approve(requestId, comment),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['requests'] })
      qc.invalidateQueries({ queryKey: ['approvals'] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      toast.success('Pedido aprovado!', 'O requerente será notificado.')
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })
}

export function useRejectRequest(requestId: string) {
  const qc = useQueryClient()
  const toast = useToast()

  return useMutation({
    mutationFn: (comment: string) => requestsApi.reject(requestId, comment),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['requests'] })
      qc.invalidateQueries({ queryKey: ['approvals'] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      toast.success('Pedido rejeitado.', 'O requerente será notificado.')
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })
}

export function useAddComment(requestId: string) {
  const qc = useQueryClient()
  const toast = useToast()

  return useMutation({
    mutationFn: (content: string) => requestsApi.addComment(requestId, content),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['requests', requestId] })
      toast.success('Comentário adicionado.')
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })
}

export function useUploadAttachment(requestId: string) {
  const qc = useQueryClient()
  const toast = useToast()

  return useMutation({
    mutationFn: (file: File) => requestsApi.uploadAttachment(requestId, file),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['requests', requestId] })
      toast.success('Ficheiro enviado!')
    },
    onError: (e) => toast.error('Erro no upload', parseApiError(e as never)),
  })
}
