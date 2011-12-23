===========================================
EBS Snapshot Utility
===========================================

Amazon EC2上のEBSスナップショットの作成・削除を自動化する為のツールです。
Winsowsのタスクスケジューラー等に登録して定期実行される様に設定して下さい。

使い方:

  Ec2EbsSnap.exe /L  volume_id                          
    ボリュームのスナップショット一覧を表示します。

  Ec2EbsSnap.exe /c  volume_id
 　 指定ボリュームのスナップショットを作成します。

  Ec2EbsSnap.exe /d  volume_id description max_generation [max_age] 
  　指定ボリュームのスナップショットの内、世代が古いものと日付が古いものを削除します。
  　日付はmax_ageパラメータで指定します。（d, h, mのいずれかを数字の後ろに付けます。）
　　例）
	  14d は 14日
	  48h は 48時間
	  90m は 90分
	  の意味になります。

  Ec2EbsSnap.exe /cd volume_id description max_generation [max_age] 
    スナップショットの作成後、削除処理を実行します。

  Ec2EbsSnap.exe /enc text_to_encrypt                   
    設定ファイルに指定するアクセスキー、シークレットキーを暗号化するのに使用します。




===========================================
このツールを動かす前に「Ec2EbsSnap.exe.config」ファイルを編集してアクセスキーとシークレットキーをセットして下さい。

  EncryptedAWSAccessKey = あなたのAWSアカウントのアクセスキーを暗号化したもの
  EncryptedAWSSecretKey = あなたのAWSアカウントのシークレットキーを暗号化したもの

アクセスキーとシークレットキーの暗号化は下のコマンドで行います。
  Ec2EbsSnap.exe /enc "[あなたのアクセスキー]"
  Ec2EbsSnap.exe /enc "[あなたのシークレットキー]"

これらの結果表示される文字列を「Ec2EbsSnap.exe.config」ファイルにセットして下さい。
===========================================




