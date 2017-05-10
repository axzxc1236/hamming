Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim hex_to_bin As String

        '把輸入值從小寫字母轉換成大寫字母並檢查是否為16位元的輸入值，同時將16進位數值轉成2進位數值
        Input.Text = Input.Text.ToUpper
        If Input.Text.StartsWith("0X") And Input.Text.Length > 2 Then
            hex_to_bin = Convert.ToString(Convert.ToInt64(Input.Text.Substring(2), 16), 2)
        Else
            Throw New System.Exception("Input is not hexadeciaml.")
        End If

        Dim answer As New Hamming(hex_to_bin)
        TextBox3.Clear()
        TextBox3.AppendText("Received data = 0x" & Convert.ToString(Convert.ToInt64(answer.received_data, 2), 16) & vbCrLf)
        TextBox3.AppendText("Received parity = " & answer.received_parity & vbCrLf)
        TextBox3.AppendText("Calculated parity = " & answer.calculated_parity & vbCrLf)
        TextBox3.AppendText("Corrected data = 0x" & Convert.ToString(Convert.ToInt64(answer.corrected_data, 2), 16) & vbCrLf)
    End Sub
End Class

Class Hamming
    Public ReadOnly received_data, received_parity, calculated_parity, corrected_data As String
    Private paritys, datas As New List(Of Integer) '紀錄糾錯碼和資料位元的位置(從右到左第n個)
    Sub New(Input_Binary As String)
        '解讀輸入值
        Input_Binary = Input_Binary.TrimStart("0") '清除漢明碼左邊的0
        Input_Binary = Input_Binary.PadLeft(3, "0") '對 0 1 10  11等資料為元為空的漢明碼作補償(補到3位元長)

        For i = 1 To Input_Binary.Length
            If Math.Log(i, 2) Mod 1 = 0 Then '第Input_Binary.Length-i個位元是否為檢查碼
                paritys.Add(i) '1,2,4,8....
                received_parity = Input_Binary(Input_Binary.Length - i) & received_parity
            Else
                datas.Add(i) '3,5,6,7,9,10...
                received_data = Input_Binary(Input_Binary.Length - i) & received_data
            End If
        Next

        '計算檢查碼
        For i = 0 To paritys.Count - 1
            Dim count As Integer = 0
            For j = 0 To datas.Count - 1
                Dim tmp As String = Convert.ToString(datas(j), 2)
                If tmp.Length >= i + 1 Then
                    If tmp(tmp.Length - 1 - i) = "1" Then '同位元檢查(是否為1)
                        If Input_Binary(Input_Binary.Length - datas(j)) = "1" Then '資料檢查
                            count += 1
                        End If
                    End If
                End If
            Next
            calculated_parity = count Mod 2 & calculated_parity
        Next

        '將兩個檢查碼做XOR運算
        Dim countP As Integer = 0 'XOR運算後的檢查碼中1的數量
        Dim XORP As String = "" '進行XOR運算後的檢查碼
        For i = 0 To received_parity.Length - 1 '從左檢查到右
            If received_parity(i) = calculated_parity(i) Then
                XORP = XORP & "0"
            Else
                XORP = XORP & "1"
                countP += 1
            End If
        Next

        corrected_data = received_data '把接收到的資料"暫時"當成正確的數值

        If countP >= 2 Then '資料位元有誤  進行更正
            Dim location As Integer = Convert.ToInt32(XORP, 2) '漢明碼從右到左第n個位元的資料有誤

            While corrected_data.Length < location '對一開始把漢明碼左邊的0消去做補償動作
                corrected_data = "0" & corrected_data
                datas.Add(datas.Last + 1)
            End While

            Dim d As Integer = datas.IndexOf(location) + 1 '第d個資料位元有誤

            Dim tmp() As Char = corrected_data.ToArray '將字串轉成字元陣列來修改其中一個字元
            If tmp(tmp.Length - d) = "1" Then '將資料位元反轉
                tmp(tmp.Length - d) = "0"
            Else
                tmp(tmp.Length - d) = "1"
            End If

            corrected_data = New String(tmp) '將陣列值轉回字串
        End If
    End Sub
End Class