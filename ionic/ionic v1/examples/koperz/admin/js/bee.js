Colony = function (location, appName) {
    if (typeof appName == 'undefined' || appName == null) {
        appName = "";
    }


    var c = {
        senderFile: function(){
          /*\
          |*|
          |*|  :: XMLHttpRequest.prototype.sendAsBinary() Polyfill ::
          |*|
          |*|  https://developer.mozilla.org/en-US/docs/DOM/XMLHttpRequest#sendAsBinary()
          \*/

          if (!XMLHttpRequest.prototype.sendAsBinary) {
           XMLHttpRequest.prototype.sendAsBinary = function(sData) {
             var nBytes = sData.length, ui8Data = new Uint8Array(nBytes);
             for (var nIdx = 0; nIdx < nBytes; nIdx++) {
               ui8Data[nIdx] = sData.charCodeAt(nIdx) & 0xff;
             }
             /* send as ArrayBufferView...: */
             this.send(ui8Data);
             /* ...or as ArrayBuffer (legacy)...: this.send(ui8Data.buffer); */
           };
          }

          /*\
          |*|
          |*|  :: AJAX Form Submit Framework ::
          |*|
          |*|  https://developer.mozilla.org/en-US/docs/DOM/XMLHttpRequest/Using_XMLHttpRequest
          |*|
          |*|  This framework is released under the GNU Public License, version 3 or later.
          |*|  https://www.gnu.org/licenses/gpl-3.0-standalone.html
          |*|
          |*|  Syntax:
          |*|
          |*|   AJAXSubmit(HTMLFormElement);
          \*/

          var AJAXSubmit = (function () {

             function ajaxSuccess () {
               /* console.log("AJAXSubmit - Success!"); */
               console.log(this.responseText);
               /* you can get the serialized data through the "submittedData" custom property: */
               /* console.log(JSON.stringify(this.submittedData)); */
             }

             function submitData (oData) {
               /* the AJAX request... */
               var oAjaxReq = new XMLHttpRequest();
               oAjaxReq.submittedData = oData;
               oAjaxReq.onload = ajaxSuccess;
               if (oData.technique === 0) {
                 /* method is GET */
                 oAjaxReq.open("get", oData.receiver.replace(/(?:\?.*)?$/,
                     oData.segments.length > 0 ? "?" + oData.segments.join("&") : ""), true);
                 oAjaxReq.send(null);
               } else {
                 /* method is POST */
                 oAjaxReq.open("post", oData.receiver, true);
                 if (oData.technique === 3) {
                   /* enctype is multipart/form-data */
                   var sBoundary = "---------------------------" + Date.now().toString(16);
                   oAjaxReq.setRequestHeader("Content-Type", "multipart\/form-data; boundary=" + sBoundary);
                   oAjaxReq.sendAsBinary("--" + sBoundary + "\r\n" +
                       oData.segments.join("--" + sBoundary + "\r\n") + "--" + sBoundary + "--\r\n");
                 } else {
                   /* enctype is application/x-www-form-urlencoded or text/plain */
                   oAjaxReq.setRequestHeader("Content-Type", oData.contentType);
                   oAjaxReq.send(oData.segments.join(oData.technique === 2 ? "\r\n" : "&"));
                 }
               }
             }

             function processStatus (oData) {
               if (oData.status > 0) { return; }
               /* the form is now totally serialized! do something before sending it to the server... */
               /* doSomething(oData); */
               /* console.log("AJAXSubmit - The form is now serialized. Submitting..."); */
               submitData (oData);
             }

             function pushSegment (oFREvt) {
               this.owner.segments[this.segmentIdx] += oFREvt.target.result + "\r\n";
               this.owner.status--;
               processStatus(this.owner);
             }

             function plainEscape (sText) {
               /* How should I treat a text/plain form encoding?
                  What characters are not allowed? this is what I suppose...: */
               /* "4\3\7 - Einstein said E=mc2" ----> "4\\3\\7\ -\ Einstein\ said\ E\=mc2" */
               return sText.replace(/[\s\=\\]/g, "\\$&");
             }

             function SubmitRequest (oTarget) {
               var nFile, sFieldType, oField, oSegmReq, oFile, bIsPost = oTarget.method.toLowerCase() === "post";
               /* console.log("AJAXSubmit - Serializing form..."); */
               this.contentType = bIsPost && oTarget.enctype ? oTarget.enctype : "application\/x-www-form-urlencoded";
               this.technique = bIsPost ?
                   this.contentType === "multipart\/form-data" ? 3 : this.contentType === "text\/plain" ? 2 : 1 : 0;
               this.receiver = oTarget.action;
               this.status = 0;
               this.segments = [];
               var fFilter = this.technique === 2 ? plainEscape : escape;
               for (var nItem = 0; nItem < oTarget.elements.length; nItem++) {
                 oField = oTarget.elements[nItem];
                 if (!oField.hasAttribute("name")) { continue; }
                 sFieldType = oField.nodeName.toUpperCase() === "INPUT" ? oField.getAttribute("type").toUpperCase() : "TEXT";
                 if (sFieldType === "FILE" && oField.files.length > 0) {
                   if (this.technique === 3) {
                     /* enctype is multipart/form-data */
                     for (nFile = 0; nFile < oField.files.length; nFile++) {
                       oFile = oField.files[nFile];
                       oSegmReq = new FileReader();
                       /* (custom properties:) */
                       oSegmReq.segmentIdx = this.segments.length;
                       oSegmReq.owner = this;
                       /* (end of custom properties) */
                       oSegmReq.onload = pushSegment;
                       this.segments.push("Content-Disposition: form-data; name=\"" +
                           oField.name + "\"; filename=\"" + oFile.name +
                           "\"\r\nContent-Type: " + oFile.type + "\r\n\r\n");
                       this.status++;
                       oSegmReq.readAsBinaryString(oFile);
                     }
                   } else {
                     /* enctype is application/x-www-form-urlencoded or text/plain or
                        method is GET: files will not be sent! */
                     for (nFile = 0; nFile < oField.files.length;
                         this.segments.push(fFilter(oField.name) + "=" + fFilter(oField.files[nFile++].name)));
                   }
                 } else if ((sFieldType !== "RADIO" && sFieldType !== "CHECKBOX") || oField.checked) {
                   /* NOTE: this will submit _all_ submit buttons. Detecting the correct one is non-trivial. */
                   /* field type is not FILE or is FILE but is empty */
                   this.segments.push(
                     this.technique === 3 ? /* enctype is multipart/form-data */
                       "Content-Disposition: form-data; name=\"" + oField.name + "\"\r\n\r\n" + oField.value + "\r\n"
                     : /* enctype is application/x-www-form-urlencoded or text/plain or method is GET */
                       fFilter(oField.name) + "=" + fFilter(oField.value)
                   );
                 }
               }
               processStatus(this);
             }

             return function (oFormElement) {
               if (!oFormElement.action) { return; }
               new SubmitRequest(oFormElement);
             };

          })();
          return  AJAXSubmit;
        },
        newBee: function (scope,autoApply) {
            if(typeof autoApply == 'undefined'){
              autoApply = false;
            }
            var b = {
                baseUrl: location + "api/Bee/",
                context: scope,
                auth: colony.auth,
                autoApply: autoApply,
                of: function (exeContext) {
                    this.context = exeContext;
                    return this;
                },
                like: function(something){
                  return "%" + something + "%";
                },
                from: function (nector, formData) {
                    var m = {
                        nector: nector,
                        baseUrl: this.baseUrl,
                        context: this.context,
                        auth: colony.auth,
                        formData:formData,
                        make: function (honeyCallBack) {
                            if (typeof this.formData == 'undefined' || this.formData == null) {
                                var nectorJson = JSON.stringify(this.nector);
                                var encoded = btoa(nectorJson);
                                $.post(this.baseUrl, { nector: encoded, way: "post", auth: this.auth }, honeyCallBack);
                            } else {
                                //we have some files to upload
                                var nectorJson = JSON.stringify(this.nector);
                                var encoded = btoa(nectorJson);
                                this.formData.append("nector",encoded);
                                this.formData.append("way","post");
                                this.formData.append("auth",this.auth);
                                var urlx = this.baseUrl;
                                $.ajax({
                                    url: urlx,
                                    type: 'POST',
                                    data: this.formData,
                                    async: false,
                                    success: function (data) {
                                        console.log("success:>", data);
                                        honeyCallBack(data);
                                    },
                                    error: function (request, status, error) {
                                        alert("Error processing request");
                                        console.log("request:>", request);
                                        console.log("status:>", status);
                                        console.log("error:>", error);
                                        honeyCallBack(null);
                                    },
                                    cache: false,
                                    contentType: false,
                                    processData: false
                                });
                            }
                        },
                    }
                    return m;
                },
                using: function (nector, formData) {
                    var m = {
                        nector: nector,
                        baseUrl: this.baseUrl,
                        context: this.context,
                        auth: colony.auth,
                        formData: formData,
                        modify: function (honeyCallBack) {
                            if (typeof this.formData == 'undefined' || this.formData == null) {
                                var nectorJson = JSON.stringify(this.nector);
                                var encoded = btoa(nectorJson);
                                $.post(this.baseUrl, { nector: encoded, way: "update", auth: this.auth }, honeyCallBack);
                            } else {
                                //we have some files to upload
                                var nectorJson = JSON.stringify(this.nector);
                                var encoded = btoa(nectorJson);
                                this.formData.append("nector",encoded);
                                this.formData.append("way", "update");
                                this.formData.append("auth",this.auth);
                                var urlx = this.baseUrl;
                                $.ajax({
                                    url: urlx,
                                    type: 'POST',
                                    data: this.formData,
                                    async: false,
                                    success: function (data) {
                                        console.log("success update data:>", data);
                                        honeyCallBack(data);
                                    },
                                    error: function (request, status, error) {
                                        alert("Error processing update request");
                                        console.log("request:>", request);
                                        console.log("status:>", status);
                                        console.log("error:>", error);
                                        honeyCallBack(null);
                                    },
                                    cache: false,
                                    contentType: false,
                                    processData: false
                                });
                            }
                        },
                    }
                    return m;
                },
                make: function (nector, honeyCallBack) {
                    var nectorJson = JSON.stringify(nector);
                    var encoded = btoa(nectorJson);
                    $.post(this.baseUrl, { nector: encoded, way: "post", auth: this.auth }, honeyCallBack);
                },
                put:function(nector,honeyCallBack){

                },
                go: function(method){
                  var exef = function (nector,formData,honeyCallBack) {
                    if(typeof honeyCallBack == 'undefined' && typeof formData == 'function'){
                      honeyCallBack = formData;
                      formData = undefined;
                    }
                    var nectorJson = JSON.stringify(nector);
                    var encoded = btoa(nectorJson);
                    var xhr = new XMLHttpRequest();
                    //console.log("the bee is",this);
                    xhr.context = this.context;
                    xhr.autoApply = this.autoApply;
                    xhr.honeyCallBack = honeyCallBack;
                    xhr.open('POST', this.baseUrl, true);
                    xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
                    if (typeof formData != 'undefined' && typeof honeyCallBack == 'function' && (method == "post" || method == "update")) {
                       //we have filed to upload
                       this.formData.append("nector",encoded);
                       this.formData.append("way", method);
                       this.formData.append("auth",this.auth);
                       var urlx = this.baseUrl;
                       $.ajax({
                           url: urlx,
                           type: 'POST',
                           data: this.formData,
                           async: false,
                           success: function (data) {
                               console.log("success update data:>", data);
                               honeyCallBack(data);
                           },
                           error: function (request, status, error) {
                               alert("Error processing update request");
                               console.log("request:>", request);
                               console.log("status:>", status);
                               console.log("error:>", error);
                               honeyCallBack(null);
                           },
                           cache: false,
                           contentType: false,
                           processData: false
                       });
                    }else{
                      var dataToSend = { nector: encoded, way: method, auth: this.auth }
                      var encodedString = '';
                      for (var prop in dataToSend) {
                          if (dataToSend.hasOwnProperty(prop)) {
                              if (encodedString.length > 0) {
                                  encodedString += '&';
                              }
                              encodedString += encodeURI(prop + '=' + dataToSend[prop]);
                          }
                      }
                      xhr.onload = function() {
                          if (this.status === 200) {
                              //console.log(xhr.responseText);
                              var data = JSON.parse(this.responseText);
                              if(typeof this.context != 'undefined' && this.autoApply == true){
                                this.context.honeyCallBack = this.honeyCallBack;
                                this.context.$apply(function (cont) {
                                  cont.honeyCallBack(data,cont);
                                });
                              }else{
                                this.honeyCallBack(data,this.context);
                              }
                          }
                          else {
                              alert('Request failed.  Returned status of ' + this.status);
                          }
                      };
                      xhr.send(encodedString);
                    }
                  };
                  return exef;
                },
                upload:function(nector,fileKey,honeyCallBack){
                    //create a pumpiing mechanism
                    var pumpMechanism = {
                      load: JSON.stringify(nector),
                      start:0,
                      stop:this.chunkSize,
                      chunkSize: this.chunkSize,
                      rounds:1,
                      hasErrors: false,
                      hasCleared: false,
                      context:this.context,
                      autoApply:this.autoApply,
                      honeyCallBack: honeyCallBack,
                      baseUrl : this.baseUrl,
                      auth: this.auth,
                      fileKey:fileKey,
                      get: this.get,
                      pump:function(pumpMechanismObject){
                        if(pumpMechanismObject.hasCleared == false){
                          //save my self as a context
                          // var temp = this.context;
                          // console.log("current context:", this.context);
                          // this.context = this;
                          // this.context.context = temp;
                          //first clear
                          pumpMechanismObject.get({_fClearChunks:{}},function(hny,context){
                            pumpMechanismObject.hasCleared =true;
                            //console.log("clear honey:", hny);
                            //console.log("clear context:", context);
                            //console.log("this pump:", pump);
                            if (hny.hasOwnProperty("_errors") && hny._errors.length > 0) {
                                console.log("Closing pump with  errors after clearing " + pumpMechanismObject.rounds);
                                pumpMechanismObject.honeyCallBack(hny._errors);
                            }else{
                              //post a chunk
                              var chunkNector = {
                                _fAddChunks:{
                                  chunk:pumpMechanismObject.load.slice(pumpMechanismObject.start, pumpMechanismObject.stop),
                                  page:pumpMechanismObject.rounds,
                                  filekey: fileKey
                                },
                                _errors:[]
                              };
                              if(chunkNector._fAddChunks.chunk.length <= 0){
                                  //seal off the deal and move on
                                  //pumpMechanismObject.honeyCallBack(hny._errors);
                                  pumpMechanismObject.get({_fMakeFile:{fileKey:fileKey},_errors:[]},function(hny,context){
                                    if(hny.hasOwnProperty("_errors") && hny._errors.length > 0){
                                      console.log("Closing pump with  errors after finished " + pumpMechanismObject.rounds);
                                      pumpMechanismObject.honeyCallBack(hny._errors);
                                    }else{
                                      console.log("Sent load for conclusion " , hny);
                                      pumpMechanismObject.honeyCallBack(hny,context);
                                    }
                                  });
                              }else{
                                //{
                                //   "Chunk": "{\"NewsFeed",
                                //   "Page": 0,
                                //   "FileKey": "Image"
                                //}
                                pumpMechanismObject.get(chunkNector,function(hny,context){
                                  pumpMechanismObject.rounds = pumpMechanismObject.rounds + 1;
                                  if(hny.hasOwnProperty("_errors") && hny._errors.length > 0){
                                    console.log("Closing pump with  errors after round " + pumpMechanismObject.rounds);
                                    pumpMechanismObject.honeyCallBack(hny._errors);
                                  }else{
                                    console.log("Sent load for " + pumpMechanismObject.rounds + " returned as " , hny.AddChunks.BeeChunk.Page);
                                    pumpMechanismObject.start = pumpMechanismObject.stop;
                                    pumpMechanismObject.stop = pumpMechanismObject.start + pumpMechanismObject.chunkSize;
                                    //repump
                                    pumpMechanismObject.pump(pumpMechanismObject);
                                  }
                                });
                              }
                            }
                          });//end pump
                        }else{
                          //post a chunk
                          var chunkNector = {
                            _fAddChunks:{
                              chunk:pumpMechanismObject.load.slice(pumpMechanismObject.start, pumpMechanismObject.stop),
                              page:pumpMechanismObject.rounds,
                              filekey: fileKey
                            },
                            _errors:[]
                          };
                          if(chunkNector._fAddChunks.chunk.length <= 0){
                              //seal off the deal and move on
                              //pumpMechanismObject.honeyCallBack(hny._errors);
                              pumpMechanismObject.get({_fMakeFile:{fileKey:fileKey},_errors:[]},function(hny,context){
                                if(hny.hasOwnProperty("_errors") && hny._errors.length > 0){
                                  console.log("Closing pump with  errors after finished " + pumpMechanismObject.rounds);
                                  pumpMechanismObject.honeyCallBack(hny._errors);
                                }else{
                                  console.log("Sent load for conclusion " , hny);
                                  pumpMechanismObject.honeyCallBack(hny,context);
                                }
                              });
                          }else{
                            //{
                            //   "Chunk": "{\"NewsFeed",
                            //   "Page": 0,
                            //   "FileKey": "Image"
                            //}
                            pumpMechanismObject.get(chunkNector,function(hny,context){
                              console.log("froms", hny);
                              pumpMechanismObject.rounds = pumpMechanismObject.rounds + 1;
                              if(hny.hasOwnProperty("_errors") && hny._errors.length > 0){
                                console.log("Closing pump with  errors after round two " + pumpMechanismObject.rounds);
                                pump.honeyCallBack(hny._errors);
                              }else{
                                console.log("Sent load for two " + pumpMechanismObject.rounds + " returned as " , hny.AddChunks.BeeChunk.Page);
                                pumpMechanismObject.start = pumpMechanismObject.stop;
                                pumpMechanismObject.stop = pumpMechanismObject.start + pumpMechanismObject.chunkSize;
                                //repump
                                pumpMechanismObject.pump(pumpMechanismObject);
                              }
                            });
                          }
                        }//end if(pumpMechanismObject.hasCleared == false){
                      }
                    };
                    pumpMechanism.pump(pumpMechanism);
                },
                uplodate:function(nector,fileKey,honeyCallBack){
                    //create a pumpiing mechanism
                    var pumpMechanism = {
                      load: JSON.stringify(nector),
                      start:0,
                      stop:this.chunkSize,
                      chunkSize: this.chunkSize,
                      rounds:1,
                      hasErrors: false,
                      hasCleared: false,
                      context:this.context,
                      autoApply:this.autoApply,
                      honeyCallBack: honeyCallBack,
                      baseUrl : this.baseUrl,
                      auth: this.auth,
                      fileKey:fileKey,
                      get: this.get,
                      pump:function(pumpMechanismObject){
                        if(pumpMechanismObject.hasCleared == false){
                          //save my self as a context
                          // var temp = this.context;
                          // console.log("current context:", this.context);
                          // this.context = this;
                          // this.context.context = temp;
                          //first clear
                          pumpMechanismObject.get({_fClearChunks:{}},function(hny,context){
                            pumpMechanismObject.hasCleared =true;
                            //console.log("clear honey:", hny);
                            //console.log("clear context:", context);
                            //console.log("this pump:", pump);
                            if (hny.hasOwnProperty("_errors") && hny._errors.length > 0) {
                                console.log("Closing pump with  errors after clearing " + pumpMechanismObject.rounds);
                                pumpMechanismObject.honeyCallBack(hny._errors);
                            }else{
                              //post a chunk
                              var chunkNector = {
                                _fAddChunks:{
                                  chunk:pumpMechanismObject.load.slice(pumpMechanismObject.start, pumpMechanismObject.stop),
                                  page:pumpMechanismObject.rounds,
                                  filekey: fileKey
                                },
                                _errors:[]
                              };
                              if(chunkNector._fAddChunks.chunk.length <= 0){
                                  //seal off the deal and move on
                                  //pumpMechanismObject.honeyCallBack(hny._errors);
                                  pumpMechanismObject.get({_fMakeFile:{fileKey:fileKey,method:"üpdate"},_errors:[]},function(hny,context){
                                    if(hny.hasOwnProperty("_errors") && hny._errors.length > 0){
                                      console.log("Closing pump with  errors after finished " + pumpMechanismObject.rounds);
                                      pumpMechanismObject.honeyCallBack(hny._errors);
                                    }else{
                                      console.log("Sent load for conclusion " , hny);
                                      pumpMechanismObject.honeyCallBack(hny,context);
                                    }
                                  });
                              }else{
                                //{
                                //   "Chunk": "{\"NewsFeed",
                                //   "Page": 0,
                                //   "FileKey": "Image"
                                //}
                                pumpMechanismObject.get(chunkNector,function(hny,context){
                                  pumpMechanismObject.rounds = pumpMechanismObject.rounds + 1;
                                  if(hny.hasOwnProperty("_errors") && hny._errors.length > 0){
                                    console.log("Closing pump with  errors after round " + pumpMechanismObject.rounds);
                                    pumpMechanismObject.honeyCallBack(hny._errors);
                                  }else{
                                    console.log("Sent load for " + pumpMechanismObject.rounds + " returned as " , hny.AddChunks.BeeChunk.Page);
                                    pumpMechanismObject.start = pumpMechanismObject.stop;
                                    pumpMechanismObject.stop = pumpMechanismObject.start + pumpMechanismObject.chunkSize;
                                    //repump
                                    pumpMechanismObject.pump(pumpMechanismObject);
                                  }
                                });
                              }
                            }
                          });//end pump
                        }else{
                          //post a chunk
                          var chunkNector = {
                            _fAddChunks:{
                              chunk:pumpMechanismObject.load.slice(pumpMechanismObject.start, pumpMechanismObject.stop),
                              page:pumpMechanismObject.rounds,
                              filekey: fileKey
                            },
                            _errors:[]
                          };
                          if(chunkNector._fAddChunks.chunk.length <= 0){
                              //seal off the deal and move on
                              //pumpMechanismObject.honeyCallBack(hny._errors);
                              pumpMechanismObject.get({_fMakeFile:{fileKey:fileKey,method:"üpdate"},_errors:[]},function(hny,context){
                                if(hny.hasOwnProperty("_errors") && hny._errors.length > 0){
                                  console.log("Closing pump with  errors after finished " + pumpMechanismObject.rounds);
                                  pumpMechanismObject.honeyCallBack(hny._errors);
                                }else{
                                  console.log("Sent load for conclusion " , hny);
                                  pumpMechanismObject.honeyCallBack(hny,context);
                                }
                              });
                          }else{
                            //{
                            //   "Chunk": "{\"NewsFeed",
                            //   "Page": 0,
                            //   "FileKey": "Image"
                            //}
                            pumpMechanismObject.get(chunkNector,function(hny,context){
                              console.log("froms", hny);
                              pumpMechanismObject.rounds = pumpMechanismObject.rounds + 1;
                              if(hny.hasOwnProperty("_errors") && hny._errors.length > 0){
                                console.log("Closing pump with  errors after round two " + pumpMechanismObject.rounds);
                                pump.honeyCallBack(hny._errors);
                              }else{
                                console.log("Sent load for two " + pumpMechanismObject.rounds + " returned as " , hny.AddChunks.BeeChunk.Page);
                                pumpMechanismObject.start = pumpMechanismObject.stop;
                                pumpMechanismObject.stop = pumpMechanismObject.start + pumpMechanismObject.chunkSize;
                                //repump
                                pumpMechanismObject.pump(pumpMechanismObject);
                              }
                            });
                          }
                        }//end if(pumpMechanismObject.hasCleared == false){
                      }
                    };
                    pumpMechanism.pump(pumpMechanism);
                },
                chunkSize : 1000,
                user: function(prop){
                  return  "_@u_" + prop;
                }
            };
            //rest
            b.post = b.go("post");
            b.get = b.go("get");
            b.put = b.go("update");

            //crud
            b.create = b.post;
            b.read = b.get;
            b.update = b.put;
            b.delete = b.go("delete");
            return b;
        },
        auth: "",
        storage: location + appName + "/"
    };
    window.colony = c;
    return c;
};
