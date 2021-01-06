#include <MsgBoxConstants.au3>
#include<Array.au3>
#include <File.au3>
#include <GDIPlus.au3>
#include <GUIMenu.au3>

Example()

Func Example()
    ; Create a constant variable in Local scope of the message to display in FileSelectFolder.
    Local Const $sMessage = "Select a folder"
	;Local Const $LucasExe = "C:\Users\Asus\Documents\LucasPure3DEditor4_5\LucasPure3DEditor4.exe"
	Local Const $LucasExe = "C:\Users\asus\Documents\RoadRage\LucasPure3DEditor4_5\LucasPure3DEditor4.exe"

    ; Display an open dialog to select a file.
    Local $sFileSelectFolder = FileSelectFolder($sMessage, "")
	;Local $sFileSelectFolder = "C:\Temp\Simpsons Road Rage Files\PS2\STECHNG.RCF"
	Local $sTextureFolder = $sFileSelectFolder & "\textures"

   ConsoleWrite("Starting Lucas Batch Export in " & $sFileSelectFolder & @CRLF)
	If NOT FileExists($sTextureFolder) Then
        ;MsgBox($MB_SYSTEMMODAL, "", "An error occurred. The directory already exists.")
        ; Create the directory.
		 DirCreate($sTextureFolder)
		 ConsoleWrite("Created texture folder " & $sTextureFolder & @CRLF)
    EndIf




    If @error Then
        ; Display the error message.
        MsgBox($MB_SYSTEMMODAL, "", "No folder was selected.")
    Else
        ; Display the selected folder.
       ; MsgBox($MB_SYSTEMMODAL, "", "You chose the following folder:" & @CRLF & $sFileSelectFolder)

		$FileList = _FileListToArray($sFileSelectFolder, "*.p3d", $FLTA_FILES)

		;_ArrayDisplay($FileList)
		 ;  Lucas' Pure3D Editor 4.5
		 Opt("WinTitleMatchMode", 2) ;1=start, 2=subStr, 3=exact, 4=advanced, -1 to -4=Nocase

		 Local $iPID = Run($LucasExe, "", @SW_SHOWMAXIMIZED)
		 Local $hWnd = WinWait("Pure3D Editor", "", 10)

		; SendKeepActive($hWnd)



		For $i = 1 to $FileList[0]

			;If $i = 1 Then
			If $i >= 1 Then
			   ConsoleWrite($i & " Opening " & $FileList[$i] & @CRLF)
			   ;MsgBox ( flag, "title", "text" [, timeout = 0 [, hwnd]] )
			   Send("^o")

			   Local $hOpen = WinWaitActive("Open", "", 0)
			   Sleep(100)

			   Send("!n")
			   Send($sFileSelectFolder & "\" & $FileList[$i])
			   Send("{ENTER}")

			  ;;Send("{TAB}{ENTER}")

			   ;Sleep(900)
			   ; window title changes to include the opened file
			   Local $hSaveAs = WinWait($FileList[$i], "", 0)

			   ;horrible hack - possible dialog box popup about corrupted Fil
			   Send("{ENTER}")
			   Sleep(100)
			   Send("{ENTER}")

;Open Save As Menu -- save as
			   Send("{LALT}")
			   Send("{DOWN}")
			   Send("{DOWN}")
			   Send("{DOWN}")
			   Send("{DOWN}")

			   Send("{ENTER}")

			   ;Sleep(300)

			   Local $hSaveAs = WinWait("Save As", "", 0)

;save as p3dxml
			   Send("{TAB}")
			   ;Sleep(300)
			   Send("{DOWN}")
			   ;Sleep(300)
			   Send("{DOWN}")
			   Send("{ENTER}")

			   Send("{ENTER}")
			   Sleep(500)
			   ;possible "Confirm Save As" - replace file box
			   If WinExists("Confirm Save As") Then

				  ;ConsoleWrite("Confirm Save As Found" & @CRLF)
				  Send("!Y")

			   EndIf

			   WinWaitActive ("Pure3D Editor" ,"",  0 )
			   Sleep(500)

			   ;try AND EXPORT THE TEXTURES
			   Send("{LALT}")
			   Send("{RIGHT}")
			   Send("{RIGHT}")
			   Send("{RIGHT}")
			   Send("{DOWN}")
			   Send("{ENTER}")

			   Sleep(500)



			   ;Not the texture exporter
			   If WinActive("Open") Then

				  ConsoleWrite($i & " Open found - no texture export " & $FileList[$i] & @CRLF)
				  Send("{ESC}")

			   EndIf

			   If WinActive("Select Folder") Then

				  ConsoleWrite($i & " Select Folder found textures for " & $FileList[$i] & @CRLF)
				  Send($sTextureFolder)
				  Send("{ENTER}")
				  Send("{ENTER}")
				  ;return

			   EndIf

			   WinWaitActive ( $FileList[$i] ,"",  0 )

			   ;MsgBox($MB_ICONINFORMATION, "", $FileList[$i] & " " & $hMenu & " " & $iFile , 1)

			EndIf

	    Next

		; ProcessClose($iPID)
; $aArray[0] = Number of Files\Folders returned
; $aArray[1] = 1st File\Folder
; $aArray[n] = nth File\Folder

    EndIf
    ProcessClose($iPID)
	MsgBox($MB_ICONINFORMATION, "", "Completed" , 5)

EndFunc   ;==>Example
