import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft, Send, CheckCircle, XCircle, Paperclip,
  MessageSquare, Clock, User, Download, Trash2, X,
} from 'lucide-react'
import { requestsApi } from '@/api/requests'
import { StatusBadge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { Textarea } from '@/components/ui/Textarea'
import { Modal, ConfirmModal } from '@/components/ui/Modal'
import { PageSpinner } from '@/components/ui/Spinner'
import { useToast } from '@/store/uiStore'
import { useCurrentUser, useIsApprover } from '@/store/authStore'
import { parseApiError } from '@/api/client'
import { formatDate, formatDateTime, fromNow, formatFileSize } from '@/utils/formatters'
import type { RequestDetail } from '@/types/request'

export default function RequestDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const toast = useToast()
  const queryClient = useQueryClient()
  const currentUser = useCurrentUser()
  const isApprover = useIsApprover()

  const [rejectOpen, setRejectOpen] = useState(false)
  const [approveOpen, setApproveOpen] = useState(false)
  const [cancelOpen, setCancelOpen] = useState(false)
  const [approveComment, setApproveComment] = useState('')
  const [rejectComment, setRejectComment] = useState('')
  const [newComment, setNewComment] = useState('')

  const { data: request, isLoading } = useQuery({
    queryKey: ['requests', id],
    queryFn: () => requestsApi.get(id!),
    enabled: !!id,
  })

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['requests', id] })
    queryClient.invalidateQueries({ queryKey: ['requests'] })
    queryClient.invalidateQueries({ queryKey: ['dashboard'] })
  }

  const submitMutation = useMutation({
    mutationFn: () => requestsApi.submit(id!),
    onSuccess: () => { toast.success('Pedido submetido!'); invalidate() },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const approveMutation = useMutation({
    mutationFn: () => requestsApi.approve(id!, approveComment || undefined),
    onSuccess: () => {
      toast.success('Pedido aprovado!', 'O requerente será notificado.')
      setApproveOpen(false)
      setApproveComment('')
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const rejectMutation = useMutation({
    mutationFn: () => requestsApi.reject(id!, rejectComment),
    onSuccess: () => {
      toast.success('Pedido rejeitado.', 'O requerente será notificado.')
      setRejectOpen(false)
      setRejectComment('')
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const cancelMutation = useMutation({
    mutationFn: () => requestsApi.cancel(id!),
    onSuccess: () => {
      toast.success('Pedido cancelado.')
      setCancelOpen(false)
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const commentMutation = useMutation({
    mutationFn: () => requestsApi.addComment(id!, newComment),
    onSuccess: () => {
      toast.success('Comentário adicionado.')
      setNewComment('')
      invalidate()
    },
    onError: (e) => toast.error('Erro', parseApiError(e as never)),
  })

  const uploadMutation = useMutation({
    mutationFn: (file: File) => requestsApi.uploadAttachment(id!, file),
    onSuccess: () => { toast.success('Ficheiro enviado!'); invalidate() },
    onError: (e) => toast.error('Erro no upload', parseApiError(e as never)),
  })

  if (isLoading) return <PageSpinner />
  if (!request) return <div className="text-center py-20 text-gray-400">Pedido não encontrado.</div>

  const isOwner = request.requester.id === currentUser?.id
  const isDraft = request.status === 'Draft'
  const isActive = request.status === 'Pending' || request.status === 'InReview'
  const isClosed = ['Approved', 'Rejected', 'Cancelled'].includes(request.status)

  // Verifica se o utilizador é o aprovador do step actual
  const isPendingApprover =
    isApprover &&
    isActive &&
    request.approvals.some(
      (a) => a.decision === null && a.approver.id === currentUser?.id,
    )

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Cabeçalho */}
      <div className="flex items-start gap-3">
        <button
          onClick={() => navigate(-1)}
          className="p-2 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 mt-0.5 shrink-0"
        >
          <ArrowLeft className="h-5 w-5" />
        </button>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-3 flex-wrap">
            <h2 className="text-xl font-bold text-gray-900 truncate">{request.title}</h2>
            <StatusBadge status={request.status} />
          </div>
          <p className="text-sm text-gray-400 mt-1">
            {request.requestType.name} · criado por{' '}
            <span className="text-gray-600">{request.requester.name}</span> ·{' '}
            {fromNow(request.createdAt)}
          </p>
        </div>

        {/* Acções principais */}
        <div className="flex gap-2 shrink-0">
          {isDraft && isOwner && (
            <>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCancelOpen(true)}
              >
                Cancelar
              </Button>
              <Button
                size="sm"
                onClick={() => submitMutation.mutate()}
                loading={submitMutation.isPending}
              >
                <Send className="h-4 w-4" />
                Submeter
              </Button>
            </>
          )}
          {isPendingApprover && (
            <>
              <Button
                variant="danger"
                size="sm"
                onClick={() => setRejectOpen(true)}
              >
                <XCircle className="h-4 w-4" />
                Rejeitar
              </Button>
              <Button
                size="sm"
                onClick={() => setApproveOpen(true)}
              >
                <CheckCircle className="h-4 w-4" />
                Aprovar
              </Button>
            </>
          )}
          {isActive && isOwner && (
            <Button variant="ghost" size="sm" onClick={() => setCancelOpen(true)}>
              <X className="h-4 w-4" />
              Cancelar pedido
            </Button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Coluna principal */}
        <div className="lg:col-span-2 space-y-6">

          {/* Descrição e campos */}
          <div className="card p-5 space-y-4">
            <h3 className="font-semibold text-gray-900 text-sm">Detalhes do pedido</h3>
            {request.description && (
              <div>
                <p className="text-xs text-gray-400 mb-1">Descrição</p>
                <p className="text-sm text-gray-700 whitespace-pre-wrap">
                  {request.description}
                </p>
              </div>
            )}
            {request.fieldValues.length > 0 && (
              <dl className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2 border-t border-gray-50">
                {request.fieldValues.map((fv) => (
                  <div key={fv.fieldId}>
                    <dt className="text-xs text-gray-400">{fv.fieldLabel}</dt>
                    <dd className="text-sm text-gray-800 font-medium mt-0.5">
                      {fv.value || '—'}
                    </dd>
                  </div>
                ))}
              </dl>
            )}
          </div>

          {/* Fluxo de aprovação */}
          {request.approvals.length > 0 && (
            <div className="card p-5">
              <h3 className="font-semibold text-gray-900 text-sm mb-4">
                Fluxo de aprovação
              </h3>
              <ApprovalTimeline approvals={request.approvals} />
            </div>
          )}

          {/* Comentários */}
          <div className="card p-5 space-y-4">
            <h3 className="font-semibold text-gray-900 text-sm flex items-center gap-2">
              <MessageSquare className="h-4 w-4 text-gray-400" />
              Comentários ({request.comments.length})
            </h3>

            {request.comments.length > 0 && (
              <ul className="space-y-3">
                {request.comments.map((c) => (
                  <li key={c.id} className="flex gap-3">
                    <div className="h-8 w-8 rounded-full bg-gray-200 flex items-center justify-center text-xs font-medium text-gray-600 shrink-0">
                      {c.author.name.charAt(0).toUpperCase()}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-baseline gap-2">
                        <span className="text-sm font-medium text-gray-900">
                          {c.author.name}
                        </span>
                        <span className="text-xs text-gray-400">
                          {fromNow(c.createdAt)}
                          {c.isEdited && ' (editado)'}
                        </span>
                      </div>
                      <p className="text-sm text-gray-700 mt-0.5 whitespace-pre-wrap">
                        {c.content}
                      </p>
                    </div>
                  </li>
                ))}
              </ul>
            )}

            {/* Novo comentário */}
            {!isClosed && (
              <div className="flex gap-3 pt-2 border-t border-gray-50">
                <div className="h-8 w-8 rounded-full bg-brand-600 flex items-center justify-center text-xs font-medium text-white shrink-0">
                  {currentUser?.name.charAt(0).toUpperCase()}
                </div>
                <div className="flex-1 space-y-2">
                  <Textarea
                    placeholder="Escreva um comentário..."
                    value={newComment}
                    onChange={(e) => setNewComment(e.target.value)}
                    rows={2}
                  />
                  <Button
                    size="sm"
                    disabled={!newComment.trim()}
                    loading={commentMutation.isPending}
                    onClick={() => commentMutation.mutate()}
                  >
                    Comentar
                  </Button>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Coluna lateral */}
        <div className="space-y-5">
          {/* Info */}
          <div className="card p-4 space-y-3">
            <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider">
              Informação
            </h3>
            <InfoRow icon={User} label="Requerente" value={request.requester.name} />
            <InfoRow icon={Clock} label="Criado em" value={formatDate(request.createdAt)} />
            {request.submittedAt && (
              <InfoRow icon={Send} label="Submetido em" value={formatDate(request.submittedAt)} />
            )}
            {request.resolvedAt && (
              <InfoRow icon={CheckCircle} label="Resolvido em" value={formatDate(request.resolvedAt)} />
            )}
            {request.currentStep && (
              <div className="pt-2 border-t border-gray-50">
                <p className="text-xs text-gray-400 mb-1">Step actual</p>
                <p className="text-sm font-medium text-amber-700 bg-amber-50 rounded-lg px-2.5 py-1.5">
                  {request.currentStep.stepOrder}. {request.currentStep.label}
                </p>
              </div>
            )}
          </div>

          {/* Anexos */}
          <div className="card p-4 space-y-3">
            <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider flex items-center gap-1.5">
              <Paperclip className="h-3.5 w-3.5" />
              Anexos ({request.attachments.length})
            </h3>

            {request.attachments.map((att) => (
              <div key={att.id} className="flex items-center gap-2 text-sm">
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-gray-700 truncate">{att.fileName}</p>
                  <p className="text-xs text-gray-400">{formatFileSize(att.sizeBytes)}</p>
                </div>
                <a
                  href={requestsApi.getAttachmentUrl(att.id)}
                  target="_blank"
                  rel="noreferrer"
                  className="p-1.5 rounded text-gray-400 hover:text-brand-600 hover:bg-gray-100"
                  title="Descarregar"
                >
                  <Download className="h-4 w-4" />
                </a>
              </div>
            ))}

            {!isClosed && (
              <label className="flex items-center gap-2 text-sm text-brand-600 hover:text-brand-700 cursor-pointer font-medium">
                <Paperclip className="h-4 w-4" />
                Anexar ficheiro
                <input
                  type="file"
                  className="hidden"
                  onChange={(e) => {
                    const file = e.target.files?.[0]
                    if (file) uploadMutation.mutate(file)
                    e.target.value = ''
                  }}
                  disabled={uploadMutation.isPending}
                />
                {uploadMutation.isPending && (
                  <span className="text-xs text-gray-400">A enviar...</span>
                )}
              </label>
            )}
          </div>
        </div>
      </div>

      {/* Modal de Aprovação */}
      <Modal
        open={approveOpen}
        onClose={() => setApproveOpen(false)}
        title="Aprovar pedido"
        description="Pode deixar um comentário opcional antes de aprovar."
        size="sm"
        footer={
          <>
            <Button variant="outline" onClick={() => setApproveOpen(false)}>
              Cancelar
            </Button>
            <Button
              onClick={() => approveMutation.mutate()}
              loading={approveMutation.isPending}
            >
              <CheckCircle className="h-4 w-4" />
              Confirmar aprovação
            </Button>
          </>
        }
      >
        <Textarea
          label="Comentário (opcional)"
          placeholder="Ex: Aprovado. Pode avançar."
          value={approveComment}
          onChange={(e) => setApproveComment(e.target.value)}
          rows={3}
        />
      </Modal>

      {/* Modal de Rejeição */}
      <Modal
        open={rejectOpen}
        onClose={() => setRejectOpen(false)}
        title="Rejeitar pedido"
        description="Indique o motivo da rejeição — é obrigatório e será comunicado ao requerente."
        size="sm"
        footer={
          <>
            <Button variant="outline" onClick={() => setRejectOpen(false)}>
              Cancelar
            </Button>
            <Button
              variant="danger"
              onClick={() => rejectMutation.mutate()}
              loading={rejectMutation.isPending}
              disabled={!rejectComment.trim()}
            >
              <XCircle className="h-4 w-4" />
              Confirmar rejeição
            </Button>
          </>
        }
      >
        <Textarea
          label="Motivo da rejeição"
          placeholder="Ex: Período coincide com projeto crítico."
          required
          value={rejectComment}
          onChange={(e) => setRejectComment(e.target.value)}
          rows={3}
        />
      </Modal>

      {/* Modal de Cancelamento */}
      <ConfirmModal
        open={cancelOpen}
        onClose={() => setCancelOpen(false)}
        onConfirm={() => cancelMutation.mutate()}
        title="Cancelar pedido"
        message="Tem a certeza que pretende cancelar este pedido? Esta acção não pode ser desfeita."
        confirmLabel="Cancelar pedido"
        danger
        loading={cancelMutation.isPending}
      />
    </div>
  )
}

// ─── Sub-componentes ──────────────────────────────────────────────────────────

function ApprovalTimeline({
  approvals,
}: {
  approvals: RequestDetail['approvals']
}) {
  return (
    <ol className="relative border-l border-gray-200 ml-3 space-y-5">
      {approvals.map((a) => {
        const isPending = a.decision === null
        const isApproved = a.decision === 'Approved'
        const isRejected = a.decision === 'Rejected'

        return (
          <li key={a.id} className="ml-5">
            {/* Ícone do step */}
            <span
              className={`absolute -left-3 flex h-6 w-6 items-center justify-center rounded-full ring-4 ring-white text-xs font-bold
                ${isPending ? 'bg-amber-100 text-amber-600' : ''}
                ${isApproved ? 'bg-green-100 text-green-600' : ''}
                ${isRejected ? 'bg-red-100 text-red-600' : ''}
              `}
            >
              {a.stepOrder}
            </span>

            <div className="ml-1">
              <div className="flex items-center gap-2 flex-wrap">
                <p className="text-sm font-medium text-gray-900">{a.stepLabel}</p>
                <span
                  className={`text-xs font-medium px-2 py-0.5 rounded-full
                    ${isPending ? 'bg-amber-100 text-amber-700' : ''}
                    ${isApproved ? 'bg-green-100 text-green-700' : ''}
                    ${isRejected ? 'bg-red-100 text-red-700' : ''}
                  `}
                >
                  {isPending ? 'Pendente' : isApproved ? 'Aprovado' : 'Rejeitado'}
                </span>
              </div>
              <p className="text-xs text-gray-400 mt-0.5">
                {a.approver.name}
                {a.decidedAt && ` · ${formatDateTime(a.decidedAt)}`}
              </p>
              {a.comment && (
                <p className="mt-1.5 text-sm text-gray-600 bg-gray-50 rounded-lg px-3 py-2 border border-gray-100">
                  "{a.comment}"
                </p>
              )}
            </div>
          </li>
        )
      })}
    </ol>
  )
}

function InfoRow({
  icon: Icon,
  label,
  value,
}: {
  icon: React.ElementType
  label: string
  value: string
}) {
  return (
    <div className="flex items-center gap-2.5">
      <Icon className="h-4 w-4 text-gray-300 shrink-0" />
      <div>
        <p className="text-xs text-gray-400">{label}</p>
        <p className="text-sm font-medium text-gray-700">{value}</p>
      </div>
    </div>
  )
}
