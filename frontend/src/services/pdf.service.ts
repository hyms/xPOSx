import { jsPDF } from 'jspdf'
import autoTable from 'jspdf-autotable'

interface Column {
  header: string
  dataKey: string
}

export const pdfService = {
  generateTablePdf(
    title: string,
    columns: Column[],
    data: any[],
    fileName: string = 'report.pdf'
  ) {
    const doc = new jsPDF()

    doc.text(title, 14, 15)

    autoTable(doc, {
      head: [columns.map(c => c.header)],
      body: data.map(row => columns.map(c => row[c.dataKey])),
      startY: 20,
    })

    doc.save(fileName)
  },
}
