Colony = function (location, appName) {
    if (typeof appName == 'undefined' || appName == null) {
        appName = "";
    }
    var c = {
        newBee: function () {
            var b = {
                baseUrl: location + "api/Bee/",
                context: null,
                auth: colony.auth,
                of: function (exeContext) {
                    this.context = exeContext;
                    return this;
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
                        get: function (honeyCallBack) {
                            var nectorJson = JSON.stringify(this.nector);
                            var encoded = btoa(nectorJson);
                            $.post(this.baseUrl, { nector: encoded, way: "get", auth: this.auth }, this.honeyCallBack);
                        }
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
                        clean: function (honeyCallBack) {
                            var nectorJson = JSON.stringify(this.nector);
                            var encoded = btoa(nectorJson);
                            $.post(this.baseUrl, { nector: encoded, way: "delete", auth: this.auth }, honeyCallBack);
                        }
                    }
                    return m;
                },
                make: function (nector, honeyCallBack) {
                    var nectorJson = JSON.stringify(nector);
                    var encoded = btoa(nectorJson);
                    $.post(this.baseUrl, { nector: encoded, way: "post", auth: this.auth }, honeyCallBack);
                },
                get: function (honeyCallBack) {
                    var f = {
                        honeyCallBack: honeyCallBack,
                        baseUrl: this.baseUrl,
                        context: this.context,
                        auth: colony.auth,
                        from: function (nector) {
                            var nectorJson = JSON.stringify(nector);
                            var encoded = btoa(nectorJson);
                            $.post(this.baseUrl, { nector: encoded, way: "get", auth: this.auth }, this.honeyCallBack);
                        }
                    }
                    return f;
                }
            };
            return b;
        },
        auth: "",
        storage: location + appName + "/"
    };
    return c;
};